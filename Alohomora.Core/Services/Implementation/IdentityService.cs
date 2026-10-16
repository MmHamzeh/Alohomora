using Alohomora.Core.Common.Configs;
using Alohomora.Core.Common.Helpers;
using Alohomora.Core.Common.Resources;
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

internal class IdentityService : IIdentityService
{
    #region Fields and Ctor

    private readonly ITokenService _tokenService;
    private readonly TokenHelper _tokenHelper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISmsService _messageService;
    private readonly IEmailService _emailService;
    private readonly IEasyCachingProvider _accessTokenCacheStore;

    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAuthOtpRepository _authOtpRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly TimeProvider _timeProvider;

    internal IdentityService(ITokenService tokenService, IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork, ISmsService messageService, IEmailService emailService, IEasyCachingProviderFactory easyCachingProviderFactory, TokenHelper tokenHelper, IPasswordHasher<User> passwordHasher, TimeProvider timeProvider)
    {
        _tokenService = tokenService;
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
        _messageService = messageService;
        _emailService = emailService;
        _tokenHelper = tokenHelper;
        _passwordHasher = passwordHasher;
        _timeProvider = timeProvider;

        _accessTokenCacheStore = easyCachingProviderFactory.GetCachingProvider(EasyCachingConfigs.AccessTokenIdStoreName);

        _refreshTokenRepository = unitOfWork.RefreshTokenRepository;
        _authOtpRepository = unitOfWork.AuthOtpRepository;
        _userRepository = unitOfWork.UserRepository;
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

        if ((user.LockoutEnabled && user.LockoutEnd > _timeProvider.GetUtcNow()) ||
            user.UserStatusId != UserStatusEnm.Active ||
            user.CanUsePassword is false)
        {
            var errorMessage = await FailedPasswordLoginAttempt(user);
            await _unitOfWork.SaveChanges(CancellationToken.None);
            return new SingleResponse<LoginVm>(errorMessage);
        }

        var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

        if (passwordVerificationResult == PasswordVerificationResult.Failed)
        {
            var errorMessage = await FailedPasswordLoginAttempt(user);
            await _unitOfWork.SaveChanges(CancellationToken.None);
            return new SingleResponse<LoginVm>(errorMessage);
        }

        if (passwordVerificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
            userHasChanged = true;
        }

        if (userHasChanged)
            await _unitOfWork.SaveChanges(CancellationToken.None);

        var tokens = await _tokenService.GenerateTokensAsync(user, dto.RememberMe);

        await _accessTokenCacheStore.SetAsync(tokens.AccessTokenId.ToString(),
            user.PublicId,
            TimeSpan.FromMinutes(ApplicationSetting.AccessTokenExpirationMinutes),
            ct);

        if (user.PhoneNumberConfirmed is false)
            dto.ReturnUrl = string.IsNullOrWhiteSpace(dto.ReturnUrl)
                ? ApplicationConfig.ConfirmPhoneNumberUrl
                : ApplicationConfig.ConfirmPhoneNumberUrlWithReturnUrl.Replace("{returnUrl}", Uri.EscapeDataString(dto.ReturnUrl));

        if (ct.IsCancellationRequested)
            return new SingleResponse<LoginVm>(ErrorMessageResource.OperationCancelled);

        LoginVm loginVm = new()
        {
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            ReturnUrl = dto.ReturnUrl
        };

        return new SingleResponse<LoginVm>(loginVm);
    }


    public async Task<ISingleResponse<LoginVm>> SendLoginOtpAsync(LoginDto dto, bool createUserIfNotExists,
        CancellationToken ct)
    {
        if (string.IsNullOrEmpty(dto.PhoneNumber))
            return new SingleResponse<LoginVm>(ErrorMessageResource.WrongPhoneNumberFormat);

        var normalizePhoneNumber = PhoneNumberHelper.NormalizePhoneNumber(dto.PhoneNumber);

        if (PhoneNumberHelper.IsValidPhoneNumber(normalizePhoneNumber) is false)
            return new SingleResponse<LoginVm>(ErrorMessageResource.WrongPhoneNumberFormat);

        var user = await _userRepository.GetByPhoneNumber(normalizePhoneNumber, enableTracking: true, ct);

        if (user is null)
        {
            if (createUserIfNotExists)
                user = await CreateUser(normalizePhoneNumber, ct);
            else
                return new SingleResponse<LoginVm>(ErrorMessageResource.UserNotFound);
        }

        var authOtp = await _authOtpRepository.GetValidByPhoneNumber(dto.PhoneNumber, ct);

        if (authOtp is null)
        {
            var otp = OtpHelper.GenerateAuthOtp();

            // Prepare a new OTP entry (don't persist until we confirm sending)
            authOtp = new AuthOtp
            {
                PublicId = Guid.CreateVersion7(),
                Code = otp,
                Expires = _timeProvider.GetUtcNow().DateTime.AddMinutes(5),
                IsUsed = false,
                UserId = user.Id,
                UserPhoneNumber = user.PhoneNumber,
                UserEmail = user.Email
            };

            // Note: do not persist yet - wait until at least one delivery channel succeeds
        }
        else
        {
            // Update expiry in-memory; persist only after successful send
            authOtp.Expires = _timeProvider.GetUtcNow().DateTime.AddMinutes(5);
            authOtp.IsUsed = false;
        }

        // Build message
        var message = GenerateOtpMessage(authOtp.Code);

        // Try sending via SMS
        var smsSent = await SendOtpSms(normalizePhoneNumber, authOtp.Code, ct);

        // Try sending via Email if available
        var emailSent = false;
        if (!string.IsNullOrEmpty(user.Email))
        {
            var subject = string.Format(IdentityResource.OtpEmailSubject, CommonResource.ApplicationName);
            emailSent = await _emailService.SendEmailAsync(user.Email, subject, message, ct);
        }

        if (!smsSent && !emailSent)
        {
            // Do not persist OTP if we couldn't deliver it
            return new SingleResponse<LoginVm>(string.Format(ErrorMessageResource.FailedToSendOtp));
        }

        // Persist OTP (either updated or new)
        if (authOtp.Id == 0)
            await _authOtpRepository.AddAsync(authOtp, ct);

        await _unitOfWork.SaveChanges(CancellationToken.None);

        var response = new SingleResponse<LoginVm>(new LoginVm
        {
            AccessToken = string.Empty, // No access token yet
            RefreshToken = string.Empty, // No refresh token yet
            ReturnUrl = string.Format(ApplicationConfig.ConfirmPhoneNumberUrlWithReturnUrl, dto.ReturnUrl),
        });

        //TODO: Remove this in production. This is only for testing purposes.
        if (ApplicationSetting.ExposeOtpInResponse)
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

        if (user.LockoutEnabled && user.LockoutEnd > _timeProvider.GetUtcNow())
        {
            var errorMessage = await FailedOtpLoginAttempt(user);
            await _unitOfWork.SaveChanges(CancellationToken.None);
            return new SingleResponse<LoginVm>(errorMessage);
        }

        if (user.LockoutEnabled && user.LockoutEnd <= _timeProvider.GetUtcNow())
        {
            user.LockoutEnabled = false;
            user.LockoutEnd = null;
        }

        if (user.UserStatusId != UserStatusEnm.Active)
        {
            var errorMessage = await FailedOtpLoginAttempt(user);
            await _unitOfWork.SaveChanges(CancellationToken.None);
            return new SingleResponse<LoginVm>(errorMessage);
        }

        var otp = await _authOtpRepository.GetValidByPhoneNumberAndCode(normalizePhoneNumber, dto.AuthOtpCode, ct);

        if (otp is null || otp.IsUsed || otp.Expires < _timeProvider.GetUtcNow())
        {
            var errorMessage = await FailedOtpLoginAttempt(user);
            await _unitOfWork.SaveChanges(CancellationToken.None);
            return new SingleResponse<LoginVm>(errorMessage);
        }

        otp.IsUsed = true;

        if (user.PhoneNumberConfirmed is false)
            user.PhoneNumberConfirmed = true;

        if (user.AccessFailedCount > 0)
            user.AccessFailedCount = 0;

        await _unitOfWork.SaveChanges(CancellationToken.None);

        var tokens = await _tokenService.GenerateTokensAsync(user, dto.RememberMe);

        await _accessTokenCacheStore.SetAsync(tokens.AccessTokenId.ToString(),
            user.PublicId,
            TimeSpan.FromMinutes(ApplicationSetting.AccessTokenExpirationMinutes),
            ct);


        if (ct.IsCancellationRequested)
            return new SingleResponse<LoginVm>(ErrorMessageResource.OperationCancelled);

        LoginVm loginVm = new()
        {
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            ReturnUrl = dto.ReturnUrl
        };

        return new SingleResponse<LoginVm>(loginVm);
    }


    public async Task<LogoutVm> LogOutAsync()
    {
        var logoutResult = new LogoutVm(ApplicationConfig.ApplicationDomain);

        if (_httpContextAccessor.HttpContext != null)
        {
            string? authHeader = _httpContextAccessor.HttpContext.Request.Headers[nameof(HttpRequestHeader.Authorization)];

            if (string.IsNullOrWhiteSpace(authHeader))
                return logoutResult;

            var accessTokenId = _tokenHelper.GetAccessTokenId(accessToken: authHeader);

            if (!accessTokenId.HasValue || accessTokenId.Value == Guid.Empty)
                return logoutResult;

            await _refreshTokenRepository.RevokeByAccessTokenId(accessTokenId.Value);
            await _unitOfWork.SaveChanges();
        }

        return logoutResult;
    }

    public async Task<ISingleResponse<LoginVm>> RefreshToken(RefreshRequestDto dto, CancellationToken ct)
    {
        var tokens = await _tokenService.RefreshTokensAsync(dto.AccessToken, dto.RefreshToken, ct);
        return new SingleResponse<LoginVm>(new LoginVm
        {
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken
        });
    }

    public async Task<ISingleResponse<LoginVm>> RegisterUserAsync(RegisterUserDto dto, CancellationToken ct)
    {
        var normalizePhoneNumber = PhoneNumberHelper.NormalizePhoneNumber(dto.PhoneNumber);

        if (PhoneNumberHelper.IsValidPhoneNumber(normalizePhoneNumber) is false)
            return new SingleResponse<LoginVm>(ErrorMessageResource.WrongPhoneNumberFormat);

        var existingUser = await _userRepository.GetByPhoneNumber(normalizePhoneNumber, enableTracking: false, ct);
        
        if (existingUser is not null)
            return new SingleResponse<LoginVm>(ErrorMessageResource.DuplicateUser);

        var user = new User
        {
            PhoneNumber = normalizePhoneNumber,
            UserName = normalizePhoneNumber,
            Email = dto.Email ?? string.Empty,
            EmailConfirmed = false,
            PhoneNumberConfirmed = false,
            CanUsePassword = true,
            UserStatusId = UserStatusEnm.Active,
            PublicId = Guid.CreateVersion7(),
            CreatedOn = _timeProvider.GetUtcNow().DateTime,
            TwoFactorEnabled = false,
            LockoutEnabled = true,
            AccessFailedCount = 0,
            LockoutEnd = null
        };

        await _userRepository.AddAsync(user, ct);
        await _unitOfWork.SaveChanges(ct);

        if (!(string.IsNullOrWhiteSpace(dto.Password)))
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
            await _unitOfWork.SaveChanges(CancellationToken.None);
        }

        // Generate tokens
        var tokens = await _tokenService.GenerateTokensAsync(user, dto.RememberMe);

        // Cache access token
        await _accessTokenCacheStore.SetAsync(tokens.AccessTokenId.ToString(),
            user.PublicId,
            TimeSpan.FromMinutes(ApplicationSetting.AccessTokenExpirationMinutes),
            ct);

        var loginVm = new LoginVm(tokens.AccessToken, tokens.RefreshToken, dto.ReturnUrl);

        return new SingleResponse<LoginVm>(loginVm) { Message = MessageResource.SuccessfulRegistrationUser };
    }

    public async Task<Response> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
            return new Response(ErrorMessageResource.WrongPhoneNumberFormat);

        var normalizePhoneNumber = PhoneNumberHelper.NormalizePhoneNumber(dto.PhoneNumber);

        if (PhoneNumberHelper.IsValidPhoneNumber(normalizePhoneNumber) is false)
            return new Response(ErrorMessageResource.WrongPhoneNumberFormat);

        var user = await _userRepository.GetByPhoneNumber(normalizePhoneNumber, enableTracking: false, ct);

        if (user is null)
            return new Response(ErrorMessageResource.UserNotFound);

        // Check if OTP already exists
        var authOtp = await _authOtpRepository.GetValidByPhoneNumber(normalizePhoneNumber, ct);

        if (authOtp is not null)
        {
            // Update existing OTP in-memory
            authOtp.Code = OtpHelper.GenerateAuthOtp();
            authOtp.Expires = _timeProvider.GetUtcNow().DateTime.AddMinutes(ApplicationSetting.OtpExpirationMinutes);
            authOtp.IsUsed = false;
        }
        else
        {
            // Create new OTP (do not persist until delivery confirmed)
            var otp = OtpHelper.GenerateAuthOtp();
            authOtp = new AuthOtp
            {
                PublicId = Guid.CreateVersion7(),
                Code = otp,
                Expires = _timeProvider.GetUtcNow().DateTime.AddMinutes(ApplicationSetting.OtpExpirationMinutes),
                IsUsed = false,
                UserId = user.Id,
                UserPhoneNumber = user.PhoneNumber,
                UserEmail = user.Email
            };
        }

        // Build message
        var message = GenerateOtpMessage(authOtp.Code);

        // Try sending via SMS
        var smsSent = await _messageService.SendMessageAsync(normalizePhoneNumber, message, ct);

        // Try sending via email if user has email
        var emailSent = false;
        if (!string.IsNullOrEmpty(user.Email))
        {
            var subject = string.Format(IdentityResource.OtpEmailSubject, CommonResource.ApplicationName);
            emailSent = await _emailService.SendEmailAsync(user.Email, subject, message, ct);
        }

        if (!smsSent && !emailSent)
            return new Response(string.Format(ErrorMessageResource.FailedToSendOtp));

        // Persist OTP only after successful delivery
        if (authOtp.Id == 0)
            await _authOtpRepository.AddAsync(authOtp, ct);

        await _unitOfWork.SaveChanges(CancellationToken.None);

        var response = new Response(MessageResource.ResetPasswordMsg);

        //TODO: Remove this in production. This is only for testing purposes.
        if (ApplicationSetting.ExposeOtpInResponse)
            response.Message = $"OTP: {authOtp.Code}";

        return response;
    }

    public async Task<Response> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.PhoneNumber) || string.IsNullOrWhiteSpace(dto.OtpCode) || string.IsNullOrWhiteSpace(dto.NewPassword))
            return new Response("All fields are required");

        var normalizePhoneNumber = PhoneNumberHelper.NormalizePhoneNumber(dto.PhoneNumber);

        if (PhoneNumberHelper.IsValidPhoneNumber(normalizePhoneNumber) is false)
            return new Response(ErrorMessageResource.WrongPhoneNumberFormat);

        var user = await _userRepository.GetByPhoneNumber(normalizePhoneNumber, enableTracking: true, ct);

        if (user is null)
            return new Response(ErrorMessageResource.UserNotFound);

        // Validate OTP
        var otp = await _authOtpRepository.GetValidByPhoneNumberAndCode(normalizePhoneNumber, dto.OtpCode, ct);

        if (otp is null || otp.IsUsed || otp.Expires < _timeProvider.GetUtcNow())
            return new Response(ErrorMessageResource.InvalidOtpLoginAttempt);

        // Mark OTP as used
        otp.IsUsed = true;

        // Update password
        user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
        user.CanUsePassword = true;
        user.AccessFailedCount = 0;
        user.LockoutEnabled = false;
        user.LockoutEnd = null;

        await _unitOfWork.SaveChanges(CancellationToken.None);

        // Send password change notification
        await SendPasswordChangedNotification(user.PhoneNumber);

        return new Response("Password reset successfully");
    }







    #region Private Methods


    private static string GenerateOtpMessage(string authOtpCode)
    {
        return string.Format(IdentityResource.OtpMessage, ApplicationConfig.ApplicationName, authOtpCode,
            ApplicationConfig.AuthDomain, authOtpCode);
    }

    private async Task<string> FailedOtpLoginAttempt(User user)
        => await HandleFailedLoginAsync(user, passwordUsed: false);

    private async Task<string> FailedPasswordLoginAttempt(User user)
        => await HandleFailedLoginAsync(user, passwordUsed: true);

    private async Task<string> HandleFailedLoginAsync(User user, bool passwordUsed, CancellationToken cancellationToken = default)
    {
        var utcNow = _timeProvider.GetUtcNow();

        RegisterFailedLoginAttempt(user);
        _ = await _unitOfWork.SaveChanges(cancellationToken);

        return ResolveFailedLoginMessage(user, passwordUsed, utcNow);
    }

    private string ResolveFailedLoginMessage(User user, bool passwordUsed, DateTimeOffset utcNow)
    {
        if (IsLockedOut(user))
        {
            var remainingTime = (user.LockoutEnd!.Value - utcNow).ToPersianRemainingTime();
            return $"حساب کاربری شما قفل شده است. لطفا بعد از {remainingTime} مجددا تلاش کنید.";
        }

        return passwordUsed
            ? ErrorMessageResource.InvalidPasswordLoginAttempt
            : ErrorMessageResource.InvalidOtpLoginAttempt;
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
            LockoutEnd = null,
            AccessFailedCount = 0,
            PublicId = Guid.CreateVersion7(),
            UserName = phoneNumber,
            CreatedOn = _timeProvider.GetUtcNow().DateTime
        };
        await _userRepository.AddAsync(user, ct);
        await _unitOfWork.SaveChanges(CancellationToken.None);
        return user;
    }

    private async Task<bool> SendOtpSms(string phoneNumber, string otp, CancellationToken ct = default)
    {
        var message = $"{CommonResource.ApplicationName}\n" +
                      string.Format(IdentityResource.OtpMessage, otp, otp, _timeProvider.GetUtcNow().DateTime.ToPersianDateTime().ToShortDateString(), _timeProvider.GetUtcNow().DateTime.ToPersianDateTime().ToLongTimeString());

        return await _messageService.SendMessageAsync(phoneNumber, message, ct);
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
        var message = string.Format(IdentityResource.AccountLockedMsg,
            lockoutEnd.ToPersianDateTime().ToShortDateString(), lockoutEnd.ToPersianDateTime().ToLongTimeString());
        return await _messageService.SendMessageAsync(phoneNumber, message);
    }

    private void RegisterFailedLoginAttempt(User user)
    {
        if (!user.LockoutEnabled) return;

        user.AccessFailedCount++;

        var utcNow = _timeProvider.GetUtcNow();

        user.LockoutEnd = user.AccessFailedCount switch
        {
            >= 15 => utcNow.AddDays(1).DateTime,
            >= 10 => utcNow.AddHours(1).DateTime,
            >= 5 => utcNow.AddMinutes(15).DateTime,
            _ => user.LockoutEnd
        };
    }

    private bool IsLockedOut(User user)
        => user is { LockoutEnabled: true, LockoutEnd: not null } && user.LockoutEnd.Value > _timeProvider.GetUtcNow();

    #endregion

}
