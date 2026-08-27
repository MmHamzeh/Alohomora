namespace Alohomora.DataAccess.Implementation.Repositories;

internal class RoleRepository : IRoleRepository
{

    #region Fields and Ctor

    private readonly DatabaseContext _dbContext;
    private readonly DatabaseContextRead _authDatabaseContextRead;

    internal RoleRepository(DatabaseContext? dbContext = null, DatabaseContextRead? authDataBaseContextRead = null)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _authDatabaseContextRead = authDataBaseContextRead ?? throw new ArgumentNullException(nameof(dbContext));
    }

    #endregion

    public async Task<IList<string>> GetUserRolesName(long userId)
    {
        return await _dbContext.UserRoles
            .Where(e => e.UserId == userId)
            .Include(e => e.Role)
            .Select(e => e.Role.Name)
            .ToListAsync();
    }
}