using Alohomora.Core.Domain.Models.DtoModels;
using Alohomora.Core.Domain.Models.ResponseModels;

namespace Alohomora.Core.Services.Contact;

public interface IRoleService
{

    // Role Management
    Task<Response> CreateRoleAsync(CreateRoleDto dto, CancellationToken ct);
    Task<Response> AssignRoleToUserAsync(AssignRoleToUserDto dto, CancellationToken ct);
    Task<Response> RemoveRoleFromUserAsync(RemoveRoleFromUserDto dto, CancellationToken ct);
    Task<ISingleResponse<List<string>>> GetUserRolesAsync(Guid userId, CancellationToken ct);
}