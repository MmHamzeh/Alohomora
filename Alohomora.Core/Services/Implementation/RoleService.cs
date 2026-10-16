using Alohomora.Core.DataAccess.Contract;
using Alohomora.Core.DataAccess.Contract.Repositories;
using Alohomora.Core.Domain.Models.DbModels;
using Alohomora.Core.Domain.Models.DtoModels;
using Alohomora.Core.Domain.Models.ResponseModels;
using Alohomora.Core.Services.Contact;

namespace Alohomora.Core.Services.Implementation;

public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;


    private readonly IRoleRepository _roleRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IUserRepository _userRepository;



    public RoleService(IUnitOfWork unitOfWork, TimeProvider timeProvider)
    {
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;

        _roleRepository = unitOfWork.RoleRepository;
        _userRoleRepository = unitOfWork.UserRoleRepository;
        _userRepository = unitOfWork.UserRepository;
    }

    public async Task<Response> CreateRoleAsync(CreateRoleDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return new Response("Role name is required");

        if (string.IsNullOrWhiteSpace(dto.FaName))
            return new Response("Role Persian name is required");

        // Check if role already exists
        var roleExists = await _roleRepository.ExistsByName(dto.Name, ct);

        if (roleExists)
            return new Response("Role with this name already exists");

        var role = new Role
        {
            PublicId = Guid.CreateVersion7(),
            Name = dto.Name,
            FaName = dto.FaName,
            Description = dto.Description ?? string.Empty,
            CreatedOn = _timeProvider.GetUtcNow().DateTime
        };

        await _roleRepository.AddAsync(role, ct);
        await _unitOfWork.SaveChanges();

        return new Response("Role created successfully");
    }

    public async Task<Response> AssignRoleToUserAsync(AssignRoleToUserDto dto, CancellationToken ct)
    {
        if (dto.UserId == Guid.Empty)
            return new Response("User ID is required");

        if (string.IsNullOrWhiteSpace(dto.RoleName))
            return new Response("Role name is required");

        // Find user by PublicId
        var user = await _userRepository.GetByIdAsync(dto.UserId, enableTracking: false, ct);
        if (user is null)
            return new Response("User not found");

        // Find role by name
        var role = await _roleRepository.GetByName(dto.RoleName, enableTracking: false, ct);

        if (role is null)
            return new Response("Role not found");

        // Check if user already has this role
        var existingUserRole = await _userRoleRepository.ExistsByUserIdRoleId(user.Id, role.Id, ct);

        if (existingUserRole is not false)
            return new Response("User already has this role");

        // Assign role to user
        var userRole = new UserRole
        {
            PublicId = Guid.CreateVersion7(),
            UserId = user.Id,
            RoleId = role.Id,
            CreatedOn = _timeProvider.GetUtcNow().DateTime
        };

        await _userRoleRepository.AddAsync(userRole, ct);
        await _unitOfWork.SaveChanges();

        return new Response("Role assigned to user successfully");
    }

    public async Task<Response> RemoveRoleFromUserAsync(RemoveRoleFromUserDto dto, CancellationToken ct)
    {
        if (dto.UserId == Guid.Empty)
            return new Response("User ID is required");

        if (string.IsNullOrWhiteSpace(dto.RoleName))
            return new Response("Role name is required");

        // Find user by PublicId
        var user = await _userRepository.GetByIdAsync(dto.UserId, enableTracking: false, ct);
        if (user is null)
            return new Response("User not found");

        // Find role by name
        var role = await _roleRepository.GetByName(dto.RoleName, enableTracking: false, ct);

        if (role is null)
            return new Response("Role not found");

        // Find user-role relationship
        var userRole = await _userRoleRepository.GetByUserIdRoleId(user.Id, role.Id, enableTracking: true, ct);

        if (userRole is null)
            return new Response("User does not have this role");

        // Remove role from user
        _userRoleRepository.Remove(userRole);
        await _unitOfWork.SaveChanges();

        return new Response("Role removed from user successfully");
    }

    public async Task<ISingleResponse<List<string>>> GetUserRolesAsync(Guid userId, CancellationToken ct)
    {
        if (userId == Guid.Empty)
            return new SingleResponse<List<string>>("User ID is required");

        // Find user by PublicId
        var user = await _userRepository.GetByIdAsync(userId, enableTracking: false, ct);
        if (user is null)
            return new SingleResponse<List<string>>("User not found");

        // Get user roles
        var roles = await _roleRepository.GetUserRolesName(user.Id);

        return new SingleResponse<List<string>>(roles.ToList());
    }


}