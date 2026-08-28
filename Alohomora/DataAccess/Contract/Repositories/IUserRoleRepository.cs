using Alohomora.Core.Domain.Models.DbModels;

namespace Alohomora.Core.DataAccess.Contract.Repositories;

public interface IUserRoleRepository : IRepository<UserRole, long>
{
    Task<List<UserRole>> GetAll(bool enableTracking);
    Task<UserRole?> GetByUserIdRoleId(long userId, long roleId, bool enableTracking, CancellationToken ct);
    Task<bool> ExistsByUserIdRoleId(long userId, long roleId, CancellationToken ct);
    Task AddAsync(UserRole userRole, CancellationToken ct);
    void Remove(UserRole userRole);
}