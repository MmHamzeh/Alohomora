using Alohomora.Core.Common.Configs;
using Alohomora.Core.Common.Helpers;
using Alohomora.Core.Common.Providers;
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
    private readonly ILookupNormalizer _normalizer;

    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAuthOtpRepository _authOtpRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ApplicationTimeProvider _timeProvider;

    internal IdentityService(ITokenService tokenService, IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork, ISmsService messageService, IEmailService emailService, IEasyCachingProviderFactory easyCachingProviderFactory, TokenHelper tokenHelper, IPasswordHasher<User> passwordHasher, ApplicationTimeProvider timeProvider, ILookupNormalizer normalizer)
    {
        _tokenService = tokenService;
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
        _messageService = messageService;
        _emailService = emailService;
        _tokenHelper = tokenHelper;
        _passwordHasher = passwordHasher;
        _timeProvider = timeProvider;
        _normalizer = normalizer;

        _accessTokenCacheStore = easyCachingProviderFactory.GetCachingProvider(EasyCachingConfigs.AccessTokenIdStoreName);

        _refreshTokenRepository = unitOfWork.RefreshTokenRepository;
        _authOtpRepository = unitOfWork.AuthOtpRepository;
        _userRepository = unitOfWork.UserRepository;
    }

    #endregion

    #region Send Login Otp

    public async Task<ISingleResponse<LoginVm>> SendLoginOtpAsync(PhoneNumberLoginOtpDto dto, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(dto.PhoneNumber))
            return new SingleResponse<LoginVm>(ErrorMessageResource.WrongPhoneNumberFormat);

        var normalizePhoneNumber = PhoneNumberHelper.NormalizePhoneNumber(dto.PhoneNumber);

        if (PhoneNumberHelper.IsValidPhoneNumber(normalizePhoneNumber) is false)
            return new SingleResponse<LoginVm>(ErrorMessageResource.WrongPhoneNumberFormat);

        var user = await _userRepository.GetByPhoneNumber(normalizePhoneNumber, enableTracking: true, ct);

        if (user is null)
        {
            if (ApplicationConfig.CreateUserIfNotExists)
                user = await CreateUser(normalizePhoneNumber, ct);
            else
                return new SingleResponse<LoginVm>(ErrorMessageResource.UserNotFound);
        }

        return await SendLoginOtpAsync(user, LoginMethodEnm.PhoneNumber, dto.ReturnUrl, ct);
    }

    public async Task<ISingleResponse<LoginVm>> SendLoginOtpAsync(EmailLoginOtpDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Email))
            return new SingleResponse<LoginVm>(ErrorMessageResource.InvalidPasswordLoginAttempt);

        if (!EmailHelper.IsValidEmail(dto.Email))
            return new SingleResponse<LoginVm>(ErrorMessageResource.InvalidPasswordLoginAttempt);

        var normalizedEmail = _normalizer.NormalizeEmail(dto.Email);

        var user = await _userRepository.GetByEmail(normalizedEmail, enableTracking: true, ct);

        if (user is null)
        {
            if (ApplicationConfig.CreateUserIfNotExists)
                user = await CreateUser(normalizedEmail, ct);
            else
                return new SingleResponse<LoginVm>(ErrorMessageResource.InvalidPasswordLoginAttempt);
        }

        return await SendLoginOtpAsync(user, LoginMethodEnm.Email, dto.ReturnUrl, ct);
    }

    public async Task<ISingleResponse<LoginVm>> SendLoginOtpAsync(UsernameLoginOtpDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Username))
            return new SingleResponse<LoginVm>(ErrorMessageResource.InvalidPasswordLoginAttempt);

        var normalizedUsername = _normalizer.NormalizeName(dto.Username);

        var user = await _userRepository.GetByUsername(normalizedUsername, enableTracking: true, ct);

        if (user is null)
        {
            if (ApplicationConfig.CreateUserIfNotExists)
                user = await CreateUser(normalizedUsername, ct);
            else
                return new SingleResponse<LoginVm>(ErrorMessageResource.InvalidPasswordLoginAttempt);
        }

        return await SendLoginOtpAsync(user, LoginMethodEnm.Username, dto.ReturnUrl, ct);
    }

    private async Task<ISingleResponse<LoginVm>> SendLoginOtpAsync(User user, LoginMethodEnm loginMethod, string? returnUrl, CancellationToken ct)
    {
        var authOtp = await _authOtpRepository.GetValidByUserPublicId(user.PublicId, ct);

        if (authOtp is null)
        {
            var otp = OtpHelper.GenerateAuthOtp();

            authOtp = new AuthOtp
            {
                PublicId = Guid.CreateVersion7(),
                Code = otp,
                Expires = _timeProvider.GetDateTime().AddMinutes(5),
                IsUsed = false,
                UserId = user.Id,
                UserPhoneNumber = user.PhoneNumber,
                UserEmail = user.Email
            };
        }
        else
        {
            // Update expiry in-memory; persist only after successful send
            authOtp.Expires = _timeProvider.GetDateTime().AddMinutes(5);
            authOtp.IsUsed = false;
        }
        
        var isOtpSent = false;

        var message = GenerateOtpMessage(authOtp.Code);
        var subject = string.Format(IdentityResource.OtpEmailSubject, CommonResource.ApplicationName);

        if (loginMethod == LoginMethodEnm.PhoneNumber)
            isOtpSent = await SendOtpSms(user.PhoneNumber, authOtp.Code, ct);
        else if (loginMethod == LoginMethodEnm.Email)
            isOtpSent = await _emailService.SendEmailAsync(user.Email, subject, message, ct);
        else 
        {
            if (user.PhoneNumberConfirmed)
                isOtpSent = await SendOtpSms(user.PhoneNumber, authOtp.Code, ct);
            if (user.EmailConfirmed)
                isOtpSent = await _emailService.SendEmailAsync(user.Email, subject, message, ct);
        }

        if (!isOtpSent)
            return new SingleResponse<LoginVm>(string.Format(ErrorMessageResource.FailedToSendOtp));

        // Persist OTP (either updated or new)
        if (authOtp.Id == 0)
            await _authOtpRepository.AddAsync(authOtp, ct);

        await _unitOfWork.SaveChanges(CancellationToken.None);

        if (string.IsNullOrWhiteSpace(returnUrl) || !ReturnUrlHelper.IsValidLocalUrl(returnUrl))
            returnUrl = string.Empty;

        var response = new SingleResponse<LoginVm>(new LoginVm
        {
            AccessToken = string.Empty, // No access token yet
            RefreshToken = string.Empty, // No refresh token yet
            ReturnUrl = string.Format(ApplicationConfig.ConfirmPhoneNumberUrlWithReturnUrl, returnUrl),
        });

        //TODO: Remove this in production. This is only for testing purposes.
        if (ApplicationSetting.ExposeOtpInResponse)
            response.Message = "Otp: " + authOtp.Code;

        return response;
    }

    private static string GenerateOtpMessage(string authOtpCode)
    {
        return string.Format(IdentityResource.OtpMessage, ApplicationConfig.ApplicationName, authOtpCode,
            ApplicationConfig.AuthDomain, authOtpCode);
    }
    
    private async Task<bool> SendOtpSms(string phoneNumber, string otp, CancellationToken ct = default)
    {
        var now = _timeProvider.GetDateTime().ToPersianDateTime();
        var shortDateString = now.ToShortDateString();
        var longTimeString = now.ToLongTimeString();

        var message = $"{CommonResource.ApplicationName}\n" +
                      string.Format(IdentityResource.OtpMessage, otp, otp, shortDateString, longTimeString);

        return await _messageService.SendMessageAsync(phoneNumber, message, ct);
    }

    #endregion

    #region Login

    #region  Login Password

    public async Task<ISingleResponse<LoginVm>> LoginPasswordAsync(PhoneNumberLoginPasswordDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.PhoneNumber) || string.IsNullOrWhiteSpace(dto.Password))
            return new SingleResponse<LoginVm>(ErrorMessageResource.InvalidPasswordLoginAttempt);

        var normalizedPhoneNumber = PhoneNumberHelper.NormalizePhoneNumber(dto.PhoneNumber);

        if (PhoneNumberHelper.IsValidPhoneNumber(normalizedPhoneNumber) is false)
            return new SingleResponse<LoginVm>(ErrorMessageResource.InvalidPasswordLoginAttempt);

        var user = await _userRepository.GetByPhoneNumber(normalizedPhoneNumber, enableTracking: true, ct);

        if (user is null)
            return new SingleResponse<LoginVm>(ErrorMessageResource.InvalidPasswordLoginAttempt);

        return await LoginPasswordAsync(user, LoginMethodEnm.PhoneNumber, dto, ct);
    }

    public async Task<ISingleResponse<LoginVm>> LoginPasswordAsync(EmailLoginPasswordDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Email))
            return new SingleResponse<LoginVm>(ErrorMessageResource.InvalidPasswordLoginAttempt);

        if (!EmailHelper.IsValidEmail(dto.Email))
            return new SingleResponse<LoginVm>(ErrorMessageResource.InvalidPasswordLoginAttempt);

        var normalizedEmail = _normalizer.NormalizeEmail(dto.Email);

        var user = await _userRepository.GetByEmail(normalizedEmail, enableTracking: true, ct);

        if (user is null)
            return new SingleResponse<LoginVm>(ErrorMessageResource.InvalidPasswordLoginAttempt);

        return await LoginPasswordAsync(user, LoginMethodEnm.PhoneNumber, dto, ct);
    }

    public async Task<ISingleResponse<LoginVm>> LoginPasswordAsync(UsernameLoginPasswordDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Username))
            return new SingleResponse<LoginVm>(ErrorMessageResource.InvalidPasswordLoginAttempt);

        var normalizedUsername = _normalizer.NormalizeName(dto.Username);

        var user = await _userRepository.GetByUsername(normalizedUsername, enableTracking: true, ct);

        if (user is null)
            return new SingleResponse<LoginVm>(ErrorMessageResource.InvalidPasswordLoginAttempt);

        return await LoginPasswordAsync(user, LoginMethodEnm.PhoneNumber, dto, ct);
    }

    private async Task<ISingleResponse<LoginVm>> LoginPasswordAsync(User user, LoginMethodEnm loginMethod, BasePasswordLoginDto dto, CancellationToken ct)
    {
        if ((user.LockoutEnabled && user.LockoutEnd > _timeProvider.GetDateTime()) ||
            user.UserStatusId != UserStatusEnm.Active ||
            user.CanUsePassword is false)
        {
            var errorMessage = await FailedPasswordLoginAttempt(user);
            await _unitOfWork.SaveChanges(CancellationToken.None);
            return new SingleResponse<LoginVm>(errorMessage);
        }

        var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash!, dto.Password!);

        if (passwordVerificationResult == PasswordVerificationResult.Failed)
        {
            var errorMessage = await FailedPasswordLoginAttempt(user);
            await _unitOfWork.SaveChanges(CancellationToken.None);
            return new SingleResponse<LoginVm>(errorMessage);
        }

        if (passwordVerificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password!);
            await _unitOfWork.SaveChanges(CancellationToken.None);
        }


        if (ct.IsCancellationRequested)
            return new SingleResponse<LoginVm>(ErrorMessageResource.OperationCancelled);

        return await LoginAsync(user, loginMethod, dto, ct);
    }

    
    private async Task<string> FailedPasswordLoginAttempt(User user)
        => await HandleFailedLoginAsync(user, passwordUsed: true);

    #endregion

    #region Confirm OTP and login

    public async Task<ISingleResponse<LoginVm>> LoginOtpConfirmAsync(PhoneNumberLoginOtpDto dto, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(dto.PhoneNumber) || string.IsNullOrEmpty(dto.AuthOtpCode))
            return new SingleResponse<LoginVm>(ErrorMessageResource.InvalidOtpLoginAttempt);

        var normalizePhoneNumber = PhoneNumberHelper.NormalizePhoneNumber(dto.PhoneNumber);

        if (PhoneNumberHelper.IsValidPhoneNumber(normalizePhoneNumber) is false)
            return new SingleResponse<LoginVm>(ErrorMessageResource.InvalidOtpLoginAttempt);

        var user = await _userRepository.GetByPhoneNumber(normalizePhoneNumber, enableTracking: true, ct);

        if (user is null)
            return new SingleResponse<LoginVm>(ErrorMessageResource.InvalidOtpLoginAttempt);

        return await LoginOtpConfirmAsync(user, LoginMethodEnm.PhoneNumber, dto, ct);
    }

    public async Task<ISingleResponse<LoginVm>> LoginOtpConfirmAsync(EmailLoginOtpDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Email))
            return new SingleResponse<LoginVm>(ErrorMessageResource.InvalidPasswordLoginAttempt);

        if (!EmailHelper.IsValidEmail(dto.Email))
            return new SingleResponse<LoginVm>(ErrorMessageResource.InvalidPasswordLoginAttempt);

        var normalizedEmail = _normalizer.NormalizeEmail(dto.Email);

        var user = await _userRepository.GetByEmail(normalizedEmail, enableTracking: true, ct);

        if (user is null)
            return new SingleResponse<LoginVm>(ErrorMessageResource.InvalidOtpLoginAttempt);

        return await LoginOtpConfirmAsync(user, LoginMethodEnm.Email, dto, ct);
    }

    public async Task<ISingleResponse<LoginVm>> LoginOtpConfirmAsync(UsernameLoginOtpDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Username))
            return new SingleResponse<LoginVm>(ErrorMessageResource.InvalidPasswordLoginAttempt);

        var normalizedUsername = _normalizer.NormalizeName(dto.Username);

        var user = await _userRepository.GetByUsername(normalizedUsername, enableTracking: true, ct);

        if (user is null)
            return new SingleResponse<LoginVm>(ErrorMessageResource.InvalidOtpLoginAttempt);

        return await LoginOtpConfirmAsync(user, LoginMethodEnm.Username, dto, ct);
    }

    private async Task<ISingleResponse<LoginVm>> LoginOtpConfirmAsync(User user, LoginMethodEnm loginMethod, BaseOtpLoginDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.AuthOtpCode))
            return new SingleResponse<LoginVm>(ErrorMessageResource.InvalidOtpLoginAttempt);

        var dtoOtpCode = dto.AuthOtpCode.Trim();

        if (user.LockoutEnabled && user.LockoutEnd > _timeProvider.GetDateTime())
        {
            var errorMessage = await FailedOtpLoginAttempt(user);
            await _unitOfWork.SaveChanges(CancellationToken.None);
            return new SingleResponse<LoginVm>(errorMessage);
        }

        if (user.UserStatusId != UserStatusEnm.Active)
        {
            var errorMessage = await FailedOtpLoginAttempt(user);
            await _unitOfWork.SaveChanges(CancellationToken.None);
            return new SingleResponse<LoginVm>(errorMessage);
        }

        var otp = await _authOtpRepository.GetValidByUserPublicIdAndCode(user.PublicId, dtoOtpCode, ct);

        if (otp is null || otp.Code != dtoOtpCode || otp.IsUsed || otp.Expires < _timeProvider.GetDateTime())
        {
            var errorMessage = await FailedOtpLoginAttempt(user);
            await _unitOfWork.SaveChanges(CancellationToken.None);
            return new SingleResponse<LoginVm>(errorMessage);
        }

        otp.IsUsed = true;
        await _unitOfWork.SaveChanges(CancellationToken.None);

        return await LoginAsync(user, loginMethod, dto, ct);
    }

    private async Task<string> FailedOtpLoginAttempt(User user)
        => await HandleFailedLoginAsync(user, passwordUsed: false);

    #endregion


    private async Task<ISingleResponse<LoginVm>> LoginAsync(User user, LoginMethodEnm loginMethod, BaseLoginDto dto, CancellationToken ct)
    {
        var needSaveChange = false;

        if (loginMethod == LoginMethodEnm.PhoneNumber && user.PhoneNumberConfirmed is false)
        {
            user.PhoneNumberConfirmed = true;
            needSaveChange = true;

            dto.ReturnUrl = string.IsNullOrWhiteSpace(dto.ReturnUrl)
                ? ApplicationConfig.ConfirmPhoneNumberUrl
                : ApplicationConfig.ConfirmPhoneNumberUrlWithReturnUrl.Replace("{returnUrl}", Uri.EscapeDataString(dto.ReturnUrl));
        }

        if (loginMethod == LoginMethodEnm.Email && user.EmailConfirmed is false)
        {
            user.EmailConfirmed = true;
            needSaveChange = true;

            dto.ReturnUrl = string.IsNullOrWhiteSpace(dto.ReturnUrl)
                ? ApplicationConfig.ConfirmEmailUrl
                : ApplicationConfig.ConfirmEmailUrlWithReturnUrl.Replace("{returnUrl}", Uri.EscapeDataString(dto.ReturnUrl));
        }

        if (user.AccessFailedCount > 0)
        {
            user.AccessFailedCount = 0;
            needSaveChange = true;
        }

        if (user.LockoutEnabled && user.LockoutEnd <= _timeProvider.GetDateTime())
        {
            user.LockoutEnd = null;
            needSaveChange = true;
        }

        if (needSaveChange)
            await _unitOfWork.SaveChanges(CancellationToken.None);

        if (ct.IsCancellationRequested)
            return new SingleResponse<LoginVm>(ErrorMessageResource.OperationCancelled);

        var tokens = await _tokenService.GenerateTokensAsync(user, dto.RememberMe);

        await _accessTokenCacheStore.SetAsync(tokens.AccessTokenId.ToString(),
            user.PublicId,
            TimeSpan.FromMinutes(ApplicationConfig.AccessTokenExpirationMinutes),
            ct);

        LoginVm loginVm = new()
        {
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            ReturnUrl = dto.ReturnUrl
        };
            
        if (ct.IsCancellationRequested)
            return new SingleResponse<LoginVm>(ErrorMessageResource.OperationCancelled);

        return new SingleResponse<LoginVm>(loginVm);
    }

    private async Task<string> HandleFailedLoginAsync(User user, bool passwordUsed, CancellationToken cancellationToken = default)
    {
        var utcNow = _timeProvider.GetDateTime();

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

    private void RegisterFailedLoginAttempt(User user)
    {
        if (!user.LockoutEnabled) return;

        user.AccessFailedCount++;

        user.LockoutEnd = user.AccessFailedCount switch
        {
            >= 15 => _timeProvider.GetDateTime().AddDays(1),
            >= 10 => _timeProvider.GetDateTime().AddHours(1),
            >= 5 => _timeProvider.GetDateTime().AddMinutes(15),
            _ => user.LockoutEnd
        };
    }

    private bool IsLockedOut(User user)
        => user is { LockoutEnabled: true, LockoutEnd: not null } && user.LockoutEnd.Value > _timeProvider.GetDateTime();
    
    #endregion

    #region Logout

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

    #endregion

    #region RefreshToken

    public async Task<ISingleResponse<LoginVm>> RefreshToken(RefreshRequestDto dto, CancellationToken ct)
    {
        var tokens = await _tokenService.RefreshTokensAsync(dto.AccessToken, dto.RefreshToken, ct);
        return new SingleResponse<LoginVm>(new LoginVm
        {
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken
        });
    }

    #endregion

    #region Register User

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
            Username = normalizePhoneNumber,
            Email = dto.Email ?? string.Empty,
            EmailConfirmed = false,
            PhoneNumberConfirmed = false,
            CanUsePassword = true,
            UserStatusId = UserStatusEnm.Active,
            PublicId = Guid.CreateVersion7(),
            CreatedOn = _timeProvider.GetDateTime(),
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
            TimeSpan.FromMinutes(ApplicationConfig.AccessTokenExpirationMinutes),
            ct);

        var loginVm = new LoginVm(tokens.AccessToken, tokens.RefreshToken, dto.ReturnUrl);

        return new SingleResponse<LoginVm>(loginVm) { Message = MessageResource.SuccessfulRegistrationUser };
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
            Username = phoneNumber,
            CreatedOn = _timeProvider.GetDateTime()
        };
        await _userRepository.AddAsync(user, ct);
        await _unitOfWork.SaveChanges(CancellationToken.None);
        return user;
    }

    #endregion

    #region ResetPassword

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
            authOtp.Expires = _timeProvider.GetDateTime().AddMinutes(ApplicationConfig.OtpExpirationMinutes);
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
                Expires = _timeProvider.GetDateTime().AddMinutes(ApplicationConfig.OtpExpirationMinutes),
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

        if (otp is null || otp.IsUsed || otp.Expires < _timeProvider.GetDateTime())
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

    #endregion

    #region Send Event Notification

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

    #endregion

}
