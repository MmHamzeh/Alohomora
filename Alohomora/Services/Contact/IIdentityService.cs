namespace Alohomora.Services.Contact;

internal interface IIdentityService
{
    Task<ISingleResponse<LoginVm>> LoginPasswordAsync(LoginPasswordDto dto, CancellationToken ct);


    Task<ISingleResponse<LoginVm>> SendLoginOtpAsync(LoginDto dto, CancellationToken ct);
    Task<ISingleResponse<LoginVm>> LoginOtpConfirmAsync(LoginOtpDto dto, CancellationToken ct);
    
    
    Task<Response> LogOutAsync();
    
    Task<ISingleResponse<LoginVm>> RefreshToken(RefreshRequestDto dto);
}