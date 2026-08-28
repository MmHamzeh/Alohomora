namespace Alohomora.Sql.Repositories;

internal class UserRoleRepository : IUserRoleRepository
{
    #region Fields and Ctor

    private readonly DatabaseContext _dbContext;

    internal UserRoleRepository(DatabaseContext? dbContext = null, DatabaseContextRead? authDataBaseContextRead = null)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    #endregion


    public async Task<List<UserRole>> GetAll(bool enableTracking)
    {
        var query = enableTracking
            ? _dbContext.UserRoles
            : _dbContext.UserRoles.AsNoTracking();

        return await query.ToListAsync();
    }

    public async Task<UserRole?> GetByUserIdRoleId(long userId, long roleId, bool enableTracking, CancellationToken ct)
    {
        var query = enableTracking
            ? _dbContext.UserRoles
            : _dbContext.UserRoles.AsNoTracking();

        return await query.FirstOrDefaultAsync(e => e.UserId == userId && e.RoleId == roleId, ct);
    }

    public async Task<bool> ExistsByUserIdRoleId(long userId, long roleId, CancellationToken ct)
    {
        return await _dbContext.UserRoles.AnyAsync(e => e.UserId == userId && e.RoleId == roleId, ct);
    }

    public async Task AddAsync(UserRole userRole, CancellationToken ct)
    {
        await _dbContext.UserRoles.AddAsync(userRole, ct);
    }

    public void Remove(UserRole userRole)
    {
        _dbContext.UserRoles.Remove(userRole);
    }
}