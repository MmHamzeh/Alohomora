using Alohomora.Core.Domain.Models.DtoModels;
using Alohomora.Core.Domain.Models.ResponseModels;
using Alohomora.Core.Domain.Models.ViewModels;

namespace Alohomora.Core.Services.Contact;

public interface IIdentityService
{
    Task<ISingleResponse<LoginVm>> LoginPasswordAsync(PhoneNumberLoginPasswordDto dto, CancellationToken ct);
    Task<ISingleResponse<LoginVm>> LoginPasswordAsync(EmailBaseLoginPasswordDto dto, CancellationToken ct);
    Task<ISingleResponse<LoginVm>> LoginPasswordAsync(UsernameBaseLoginPasswordDto dto, CancellationToken ct);

    Task<ISingleResponse<LoginVm>> SendLoginOtpAsync(PhoneNumberLoginOtpDto dto, CancellationToken ct);
    Task<ISingleResponse<LoginVm>> SendLoginOtpAsync(EmailBaseLoginOtpDto dto, CancellationToken ct);
    Task<ISingleResponse<LoginVm>> SendLoginOtpAsync(UsernameBaseLoginOtpDto dto, CancellationToken ct);

    Task<ISingleResponse<LoginVm>> LoginOtpConfirmAsync(PhoneNumberLoginOtpDto dto, CancellationToken ct);
    Task<ISingleResponse<LoginVm>> LoginOtpConfirmAsync(EmailBaseLoginOtpDto dto, CancellationToken ct);
    Task<ISingleResponse<LoginVm>> LoginOtpConfirmAsync(UsernameBaseLoginOtpDto dto, CancellationToken ct);
    
    
    Task<LogoutVm> LogOutAsync();
    
    Task<ISingleResponse<LoginVm>> RefreshToken(RefreshRequestDto dto, CancellationToken ct);

    // User Registration
    Task<ISingleResponse<LoginVm>> RegisterUserAsync(RegisterUserDto dto, CancellationToken ct);

    // Password Reset
    Task<Response> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken ct);
    Task<Response> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken ct);

}