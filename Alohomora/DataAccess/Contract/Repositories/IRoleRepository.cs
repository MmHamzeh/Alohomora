namespace Alohomora.DataAccess.Contract.Repositories;

internal interface IRoleRepository : IRepository<Role, long>
{
    Task<IList<string>> GetUserRolesName(long userId);
}