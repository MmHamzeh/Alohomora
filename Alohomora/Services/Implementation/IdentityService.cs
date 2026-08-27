using Alohomora.Core.Common.Configs;
using Alohomora.Core.Common.Helpers;
using Alohomora.Core.DataAccess.Contract;
using Alohomora.Core.DataAccess.Contract.Repositories;
using Alohomora.Core.Domain.Enums;
using Alohomora.Core.Domain.Models.DbModels;
using Alohomora.Core.Domain.Models.DtoModels;
using Alohomora.Core.Domain.Models.ResponseModels;
using Alohomora.Core.Domain.Models.ViewModels;
using Alohomora.Core.Services.Contact;
using Alohomora.Core.Services.Contact.ExternalServices;

namespace Alohomora.Core.Services.Implementation;

public class IdentityService : IIdentityService
{
    #region Fields and Ctor

    private readonly ITokenService _tokenService;
    private readonly TokenHelper _tokenHelper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISmsService _messageService;
    private readonly IEasyCachingProvider _accessTokenCache;

    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAuthOtpRepository _authOtpRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IPasswordHasher<User> _passwordHasher;


    internal IdentityService(ITokenService tokenService, IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork, ISmsService messageService, IEasyCachingProviderFactory easyCachingProviderFactory, TokenHelper tokenHelper)
    {
        _tokenService = tokenService;
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
        _messageService = messageService;
        _tokenHelper = tokenHelper;
        _accessTokenCache = easyCachingProviderFactory.GetCachingProvider(EasyCachingConfigs.AccessTokenIdStoreName);

        _refreshTokenRepository = unitOfWork.RefreshTokenRepository;
        _authOtpRepository = unitOfWork.AuthOtpRepository;
        _userRepository = unitOfWork.UserRepository;
        _roleRepository = unitOfWork.RoleRepository;
        _userRoleRepository = unitOfWork.UserRoleRepository;

        _passwordHasher = new PasswordHasher<User>();

    }

    #endregion


    public async Task<ISingleResponse<LoginVm>> LoginPasswordAsync(LoginPasswordDto dto, CancellationToken ct)
    {
        var userHasChanged = false;

        if (string.IsNullOrWhiteSpace(dto.PhoneNumber) || string.IsNullOrWhiteSpace(dto.Password))
            return new SingleResponse<LoginVm>(ErrorMessageResource.InvalidPasswordLoginAttempt);

        var normalizePhoneNumber = PhoneNumberHelper.NormalizePhoneNumber(dto.PhoneNumber);

        if (PhoneNumberHelper.IsValidPhoneNumber(normalizePhoneNumber) is false)
            return new SingleResponse<LoginVm>(ErrorMessageResource.InvalidPasswordLoginAttempt);

        var user = await _userRepository.GetByPhoneNumber(normalizePhoneNumber, enableTracking: true, ct);

        if (user is null)
            return new SingleResponse<LoginVm>(ErrorMessageResource.InvalidPasswordLoginAttempt);

        if (user.LockoutEnabled && user.LockoutEnd < DateTime.Now)
            return await FailedPasswordLoginAttempt(user);

        if (user.LockoutEnabled && user.LockoutEnd >= DateTime.Now)
        {
            user.LockoutEnabled = false;
            userHasChanged = true;
        }

        if (user.UserStatusId != UserStatusEnm.Active)
            return await FailedPasswordLoginAttempt(user);

        if (user.CanUsePassword is false)
            return await FailedPasswordLoginAttempt(user);

        var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

        if (passwordVerificationResult == PasswordVerificationResult.Failed)
            return await FailedPasswordLoginAttempt(user);

        if (passwordVerificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
            userHasChanged = true;
        }

        var tokens = await _tokenService.GenerateTokensAsync(user, dto.RememberMe);

        await _accessTokenCache.SetAsync(tokens.AccessTokenId.ToString(),
            user.PublicId,
            TimeSpan.FromMinutes(ApplicationSetting.AccessTokenExpirationMinutes),
            ct);

        if (user.PhoneNumberConfirmed is false)
            dto.ReturnUrl = "/Auth/ConfirmPhoneNumber?returnUrl=" + Uri.EscapeDataString(dto.ReturnUrl);

        if (userHasChanged)
            await _unitOfWork.SaveChanges();

        LoginVm loginVm = new()
        {
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            ReturnUrl = dto.ReturnUrl
        };

        return new SingleResponse<LoginVm>(loginVm);
    }


    public async Task<ISingleResponse<LoginVm>> SendLoginOtpAsync(LoginDto dto, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(dto.PhoneNumber))
            return new SingleResponse<LoginVm>(ErrorMessageResource.WrongPhoneNumberFormat);

        var normalizePhoneNumber = PhoneNumberHelper.NormalizePhoneNumber(dto.PhoneNumber);

        if (PhoneNumberHelper.IsValidPhoneNumber(normalizePhoneNumber) is false)
            return new SingleResponse<LoginVm>(ErrorMessageResource.WrongPhoneNumberFormat);

        var user = await _userRepository.GetByPhoneNumber(normalizePhoneNumber, enableTracking: true, ct);

        user ??= await CreateUser(normalizePhoneNumber, ct);

        var authOtp = await _authOtpRepository.GetValidByPhoneNumber(dto.PhoneNumber, ct);

        if (authOtp is not null)
        {
            // If an OTP already exists for this phone number, we can update it
            authOtp.Expires = DateTime.Now.AddMinutes(5);
        }
        else
        {
            var otp = OtpHelper.GenerateAuthOtp();

            // Create a new OTP entry
            authOtp = new AuthOtp
            {
                PublicId = Guid.NewGuid(),
                Code = otp,
                Expires = DateTime.Now.AddMinutes(5),
                IsUsed = false,
                UserId = user.Id,
                UserPhoneNumber = user.PhoneNumber,
                UserEmail = user.Email
            };

            await _authOtpRepository.AddAsync(authOtp, ct);
        }

        await _unitOfWork.SaveChanges();

        var sendOtpSmsResult = await SendOtpSms(normalizePhoneNumber, authOtp.Code);

        var response = new SingleResponse<LoginVm>(new LoginVm
        {
            AccessToken = string.Empty, // No access token yet
            RefreshToken = string.Empty, // No refresh token yet
            ReturnUrl = "/Login-Confirm?returnUrl=" + dto.ReturnUrl,
        });

        if (ApplicationSetting.IsDebugMode)
            response.Message = "Otp: " + authOtp.Code;

        return response;
    }

    public async Task<ISingleResponse<LoginVm>> LoginOtpConfirmAsync(LoginOtpDto dto, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(dto.PhoneNumber) || string.IsNullOrEmpty(dto.AuthOtpCode))
            return new SingleResponse<LoginVm>(ErrorMessageResource.InvalidOtpLoginAttempt);

        var normalizePhoneNumber = PhoneNumberHelper.NormalizePhoneNumber(dto.PhoneNumber);

        if (PhoneNumberHelper.IsValidPhoneNumber(normalizePhoneNumber) is false)
            return new SingleResponse<LoginVm>(ErrorMessageResource.InvalidOtpLoginAttempt);

        var user = await _userRepository.GetByPhoneNumber(normalizePhoneNumber, enableTracking: true, ct);

        if (user is null)
            return new SingleResponse<LoginVm>(ErrorMessageResource.InvalidOtpLoginAttempt);

        if (user.LockoutEnabled && user.LockoutEnd > DateTime.Now)
            return await FailedOtpLoginAttempt(user);

        if (user.LockoutEnabled && user.LockoutEnd <= DateTime.Now)
        {
            user.LockoutEnabled = false;
            user.LockoutEnd = null;
        }

        if (user.UserStatusId != UserStatusEnm.Active)
            return await FailedOtpLoginAttempt(user);

        var otp = await _authOtpRepository.GetValidByPhoneNumberAndCode(normalizePhoneNumber, dto.AuthOtpCode, ct);

        if (otp is null)
            return await FailedOtpLoginAttempt(user);

        if (otp.IsUsed)
            return await FailedOtpLoginAttempt(user);

        if (otp.Expires > DateTime.Now)
            return await FailedOtpLoginAttempt(user);

        otp.IsUsed = true;

        if (user.PhoneNumberConfirmed is false)
            user.PhoneNumberConfirmed = true;

        if (user.AccessFailedCount > 0)
            user.AccessFailedCount = 0;

        await _unitOfWork.SaveChanges();

        var tokens = await _tokenService.GenerateTokensAsync(user, dto.RememberMe);

        await _accessTokenCache.SetAsync(tokens.AccessTokenId.ToString(),
            user.PublicId,
            TimeSpan.FromMinutes(ApplicationSetting.AccessTokenExpirationMinutes),
            ct);

        LoginVm loginVm = new()
        {  
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            ReturnUrl = dto.ReturnUrl
        };

        return new SingleResponse<LoginVm>(loginVm);
    }


    public async Task<Response> LogOutAsync()
    {
        string? authHeader = _httpContextAccessor.HttpContext.Request.Headers[HttpRequestHeader.Authorization.ToString()];

        if (string.IsNullOrWhiteSpace(authHeader))
            return new Response();

        var accessTokenId = _tokenHelper.GetAccessTokenId(accessToken: authHeader);

        if (!accessTokenId.HasValue || accessTokenId.Value == Guid.Empty)
            return new Response();

        await _refreshTokenRepository.RevokeByAccessTokenId(accessTokenId.Value);
        await _unitOfWork.SaveChanges();

        return new Response();
    }

    public async Task<ISingleResponse<LoginVm>> RefreshToken(RefreshRequestDto dto)
    {
        var tokens = await _tokenService.RefreshTokensAsync(dto.AccessToken, dto.RefreshToken);
        return new SingleResponse<LoginVm>(new LoginVm()
        {
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken
        });
    }








    #region Private Methods

    private async Task<SingleResponse<LoginVm>> FailedOtpLoginAttempt(User user)
        => await FailedLoginAttempt(user, passwordUsed: false);

    private async Task<SingleResponse<LoginVm>> FailedPasswordLoginAttempt(User user)
        => await FailedLoginAttempt(user, passwordUsed: true);


    private async Task<SingleResponse<LoginVm>> FailedLoginAttempt(User user, bool passwordUsed)
    {
        if (user.LockoutEnabled)
        {
            ++user.AccessFailedCount;

            user.LockoutEnd = user.AccessFailedCount switch
            {
                >= 5 and < 10 => DateTime.Now.AddMinutes(15),
                >= 10 and < 15 => DateTime.Now.AddMinutes(60),
                >= 15 => DateTime.Now.AddDays(1),
                _ => user.LockoutEnd
            };

            await _unitOfWork.SaveChanges();
        }

        string errorMessage;
        string timeToDisableLockoutMessage;
        var lockoutTotalMinutes = (user.LockoutEnd! - DateTime.Now).Value.TotalMinutes;
        var lockoutTotalHours = (user.LockoutEnd! - DateTime.Now).Value.TotalHours;

        if (lockoutTotalMinutes < 60)
            timeToDisableLockoutMessage = lockoutTotalMinutes + 1 + " دقیقه،";

        else if (lockoutTotalMinutes > 60 && lockoutTotalHours < 24)
        {
            if (lockoutTotalMinutes % 60 == 0)
                timeToDisableLockoutMessage = lockoutTotalHours + " ساعت،";
            else
                timeToDisableLockoutMessage = lockoutTotalHours + "ساعت و " + lockoutTotalMinutes % 60 + " دقیقه، ";
        }
        else
        {
            var lockoutTotalDays = (user.LockoutEnd! - DateTime.Now).Value.TotalDays;


            if (lockoutTotalHours % 24 == 0 && lockoutTotalMinutes % (24 * 60) == 0)
                timeToDisableLockoutMessage = lockoutTotalDays + " روز،";
            else if (lockoutTotalHours % 24 > 0 && lockoutTotalMinutes % (24 * 60) == 0)
                timeToDisableLockoutMessage = lockoutTotalHours + " ساعت،";
            else if (lockoutTotalHours % 24 == 0 && lockoutTotalMinutes % (24 * 60) > 0)
                timeToDisableLockoutMessage = lockoutTotalHours + " ساعت،";
            else
                timeToDisableLockoutMessage = lockoutTotalHours + "ساعت و " + lockoutTotalMinutes % 60 + " دقیقه، ";
        }


        if (user.LockoutEnabled && user.LockoutEnd > DateTime.Now)
            errorMessage = "حساب کاربری شما قفل شده است. لطفا بعد از " + timeToDisableLockoutMessage + " مجددا تلاش کنید";
        //else if (user.UserStatusId != UserStatusEnm.Active)
        //    errorMessage = "حساب شما ";
        else if (passwordUsed)
            errorMessage = ErrorMessageResource.InvalidPasswordLoginAttempt;
        else
            errorMessage = ErrorMessageResource.InvalidOtpLoginAttempt;

        return new SingleResponse<LoginVm>(errorMessage);

    }

    private async Task<User> CreateUser(string phoneNumber, CancellationToken ct)
    {
        var user = new User
        {
            PhoneNumber = phoneNumber,
            PasswordHash = string.Empty, // Password will be set later
            CanUsePassword = false,
            UserStatusId = UserStatusEnm.Active,
            PhoneNumberConfirmed = false,
            LockoutEnabled = true,
            AccessFailedCount = 0,
            PublicId = Guid.NewGuid(),
            UserName = phoneNumber,
            CreatedOn = DateTime.Now
        };
        await _userRepository.AddAsync(user, ct);
        await _unitOfWork.SaveChanges();
        return user;
    }

    private async Task<bool> SendOtpSms(string phoneNumber, string otp)
    {
        var message = $"{CommonResource.ApplicationName}\n" +
                      string.Format(IdentityResource.OtpMessage, otp, otp, DateTime.Now.ToPersianDateTime().ToShortDateString(), DateTime.Now.ToPersianDateTime().ToLongTimeString());

        return await _messageService.SendMessageAsync(phoneNumber, message);
    }

    private async Task<bool> SendLoginNotification(string phoneNumber, string ipAddress)
    {
        var message = string.Format($"{CommonResource.ApplicationName}\n {IdentityResource.LoginNotificationWithIpAddress}", ipAddress);
        return await _messageService.SendMessageAsync(phoneNumber, message);
    }

    private async Task<bool> SendPasswordChangedNotification(string phoneNumber)
    {
        var message = $"{CommonResource.ApplicationName}\n" + IdentityResource.PasswordChangedNotification;
        return await _messageService.SendMessageAsync(phoneNumber, message);
    }

    private async Task<bool> SendInvalidLoginAttemptNotification(string phoneNumber, string ipAddress)
    {
        var message = string.Format($"{CommonResource.ApplicationName}\n {IdentityResource.InvalidLoginNotificationWithIpAddress}", ipAddress);
        return await _messageService.SendMessageAsync(phoneNumber, message);
    }

    private async Task<bool> SendAccountLockedNotification(string phoneNumber, DateTime lockoutEnd)
    {
        var message = $"{CommonResource.ApplicationName}\n" + string.Format(IdentityResource.AccountLocked, lockoutEnd.ToPersianDateTime().ToShortDateString(), lockoutEnd.ToPersianDateTime().ToLongTimeString());
        return await _messageService.SendMessageAsync(phoneNumber, message);
    }

    #endregion

}