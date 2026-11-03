using Alohomora.Core.Domain.Models.DtoModels;
using Alohomora.Core.Domain.Models.ResponseModels;
using Alohomora.Core.Domain.Models.ViewModels;

namespace Alohomora.Core.Services.Contact;

public interface IIdentityService
{
    Task<ISingleResponse<LoginVm>> LoginPasswordAsync(PhoneNumberLoginPasswordDto dto, CancellationToken ct);
    Task<ISingleResponse<LoginVm>> LoginPasswordAsync(EmailLoginPasswordDto dto, CancellationToken ct);
    Task<ISingleResponse<LoginVm>> LoginPasswordAsync(UsernameLoginPasswordDto dto, CancellationToken ct);

    Task<ISingleResponse<LoginVm>> SendLoginOtpAsync(PhoneNumberLoginOtpDto dto, CancellationToken ct);
    Task<ISingleResponse<LoginVm>> SendLoginOtpAsync(EmailLoginOtpDto dto, CancellationToken ct);
    Task<ISingleResponse<LoginVm>> SendLoginOtpAsync(UsernameLoginOtpDto dto, CancellationToken ct);

    Task<ISingleResponse<LoginVm>> LoginOtpConfirmAsync(PhoneNumberLoginOtpDto dto, CancellationToken ct);
    Task<ISingleResponse<LoginVm>> LoginOtpConfirmAsync(EmailLoginOtpDto dto, CancellationToken ct);
    Task<ISingleResponse<LoginVm>> LoginOtpConfirmAsync(UsernameLoginOtpDto dto, CancellationToken ct);
    
    
    Task<LogoutVm> LogOutAsync();
    
    Task<ISingleResponse<LoginVm>> RefreshToken(RefreshRequestDto dto, CancellationToken ct);

    // User Registration
    Task<ISingleResponse<LoginVm>> RegisterUserAsync(RegisterUserDto dto, CancellationToken ct);

    // Password Reset
    Task<Response> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken ct);
    Task<Response> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken ct);

}