namespace Alohomora.Sql.Repositories;

internal class RoleRepository : IRoleRepository
{

    #region Fields and Ctor

    private readonly DatabaseContext _dbContext;

    internal RoleRepository(DatabaseContext? dbContext = null)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
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

    public async Task AddAsync(Role role, CancellationToken ct)
    {
        await _dbContext.Roles.AddAsync(role, ct);
    }

    public async Task<List<Role>> GetAll(bool enableTracking)
    {
        var query = enableTracking
            ? _dbContext.Roles.AsQueryable()
            : _dbContext.Roles.AsNoTracking().AsQueryable();

        return await query.ToListAsync();
    }

    public async Task<Role?> GetByName(string dtoRoleName, bool enableTracking, CancellationToken ct)
    {
        var query = enableTracking
            ? _dbContext.Roles.AsQueryable()
            : _dbContext.Roles.AsNoTracking().AsQueryable();

        return await query.FirstOrDefaultAsync(e => e.Name == dtoRoleName, ct);
    }

    public async Task<bool> ExistsByName(string dtoRoleName, CancellationToken ct)
    {
        return await _dbContext.Roles.AnyAsync(e => e.Name == dtoRoleName, ct);
    }
}