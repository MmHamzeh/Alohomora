using Alohomora.Core.Domain.Models.DtoModels;
using Alohomora.Core.Domain.Models.ResponseModels;
using Alohomora.Core.Domain.Models.ViewModels;

namespace Alohomora.Core.Services.Contact;

public interface IIdentityService
{
    Task<ISingleResponse<LoginVm>> LoginPasswordAsync(LoginPasswordDto dto, CancellationToken ct);


    Task<ISingleResponse<LoginVm>> SendLoginOtpAsync(LoginDto dto, CancellationToken ct);
    Task<ISingleResponse<LoginVm>> LoginOtpConfirmAsync(LoginOtpDto dto, CancellationToken ct);
    
    
    Task<Response> LogOutAsync();
    
    Task<ISingleResponse<LoginVm>> RefreshToken(RefreshRequestDto dto, CancellationToken ct);

    // User Registration
    Task<ISingleResponse<LoginVm>> RegisterUserAsync(RegisterUserDto dto, CancellationToken ct);

    // Password Reset
    Task<Response> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken ct);
    Task<Response> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken ct);

    // Role Management
    Task<Response> CreateRoleAsync(CreateRoleDto dto, CancellationToken ct);
    Task<Response> AssignRoleToUserAsync(AssignRoleToUserDto dto, CancellationToken ct);
    Task<Response> RemoveRoleFromUserAsync(RemoveRoleFromUserDto dto, CancellationToken ct);
    Task<ISingleResponse<List<string>>> GetUserRolesAsync(Guid userId, CancellationToken ct);
}