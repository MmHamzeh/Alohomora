namespace Alohomora.DataAccess.Contract.Repositories;

internal interface IUserRepository : IRepository<User, long>
{
    Task<User?> GetByPhoneNumber(string phoneNumber, bool enableTracking, CancellationToken ct);
    Task<bool> ExistsByPhoneNumber(string phoneNumber, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
    Task<User?> GetByPublicId(Guid userPublicId, bool enableTracking);
    Task<User?> GetById(long userId, bool enableTracking);
}