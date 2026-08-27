namespace Alohomora.DataAccess.Implementation.Repositories;

internal class UserRepository : IUserRepository
{
    #region Fields and Ctor

    private readonly DatabaseContext _dbContext;
    private readonly DatabaseContextRead _dbContextRead;

    internal UserRepository(DatabaseContext? dbContext = null, DatabaseContextRead? dbContextRead = null)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dbContextRead = dbContextRead ?? throw new ArgumentNullException(nameof(dbContextRead));
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
            : _dbContextRead.Users;

        return await users.FirstOrDefaultAsync(e => e.PhoneNumber == phoneNumber, ct);
    }

    public async Task<User?> GetByPublicId(Guid userPublicId, bool enableTracking)
    {
        var users = enableTracking
            ? _dbContext.Users
            : _dbContextRead.Users;

        return await users.FirstOrDefaultAsync(e => e.PublicId == userPublicId);
    }

    public async Task<User?> GetById(long userId, bool enableTracking)
    {
        var users = enableTracking
            ? _dbContext.Users
            : _dbContextRead.Users;

        return await users.FindAsync(userId);
    }
}