using Alohomora.Core.Domain.Models.DbModels;

namespace Alohomora.Core.DataAccess.Contract.Repositories;

public interface IUserRepository : IRepository<User, long>
{
    Task<User?> GetByPhoneNumber(string phoneNumber, bool enableTracking, CancellationToken ct);
    Task<bool> ExistsByPhoneNumber(string phoneNumber, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
    Task<User?> GetByIdAsync(long userId, bool enableTracking, CancellationToken ct);
    Task<User?> GetByIdAsync(Guid userPublicId, bool enableTracking, CancellationToken ct);
}