using Alohomora.Core.Domain.Models.DbModels;

namespace Alohomora.Core.DataAccess.Contract.Repositories;

public interface IRoleRepository : IRepository<Role, long>
{
    Task<IList<string>> GetUserRolesName(long userId);
    Task AddAsync(Role role, CancellationToken ct);
    Task<List<Role>> GetAll(bool enableTracking);
    Task<Role?> GetByName(string dtoRoleName, bool enableTracking, CancellationToken ct);
    Task<bool> ExistsByName(string dtoRoleName, CancellationToken ct);
}