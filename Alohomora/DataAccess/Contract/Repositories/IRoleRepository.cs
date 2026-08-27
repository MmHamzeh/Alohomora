using Alohomora.Core.Domain.Models.DbModels;

namespace Alohomora.Core.DataAccess.Contract.Repositories;

public interface IRoleRepository : IRepository<Role, long>
{
    Task<IList<string>> GetUserRolesName(long userId);
}