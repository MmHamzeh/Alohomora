namespace Alohomora.Sql.Repositories;

internal class UserRepository : IUserRepository
{
    #region Fields and Ctor

    private readonly DatabaseContext _dbContext;

    internal UserRepository(DatabaseContext? dbContext = null)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    #endregion

    public async Task AddAsync(User user, CancellationToken ct)
    {
        await _dbContext.Users.AddAsync(user, ct);
    }

    public async Task<bool> ExistsByPhoneNumber(string phoneNumber, CancellationToken ct)
    {
        return await _dbContext.Users.AnyAsync(e => e.PhoneNumber == phoneNumber, ct);
    }


    public async Task<User?> GetByPhoneNumber(string phoneNumber, bool enableTracking, CancellationToken ct)
    {
        var users = enableTracking
            ? _dbContext.Users
            : _dbContext.Users.AsNoTracking();

        return await users.FirstOrDefaultAsync(e => e.PhoneNumber == phoneNumber, ct);
    }

    public async Task<User?> GetByPublicId(Guid userPublicId, bool enableTracking, CancellationToken ct)
    {
        var users = enableTracking
            ? _dbContext.Users
            : _dbContext.Users.AsNoTracking();

        return await users.FirstOrDefaultAsync(e => e.PublicId == userPublicId, ct);
    }

    public async Task<User?> GetByIdAsync(long userId, bool enableTracking, CancellationToken ct)
    {
        var users = enableTracking
            ? _dbContext.Users
            : _dbContext.Users.AsNoTracking();

        return await users.FirstOrDefaultAsync(e => e.Id == userId, ct);
    }

    public async Task<User?> GetByIdAsync(Guid userPublicId, bool enableTracking, CancellationToken ct)
    {
        var users = enableTracking
            ? _dbContext.Users
            : _dbContext.Users.AsNoTracking();

        return await users.FirstOrDefaultAsync(e => e.PublicId == userPublicId, ct);
    }
}