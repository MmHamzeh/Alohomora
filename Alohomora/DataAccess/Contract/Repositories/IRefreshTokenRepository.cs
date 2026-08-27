namespace Alohomora.DataAccess.Contract.Repositories;

internal interface IRefreshTokenRepository : IRepository<RefreshToken, long>
{
    Task<RefreshToken?> GetActiveByUserPublicId(Guid userPublicId, bool enableTracking,
        CancellationToken ct);
    Task RevokeByAccessTokenId(Guid accessTokenId);
    Task AddAsync(RefreshToken refreshToken);
    Task<RefreshToken?> GetByTokenAccessTokenId(string token, Guid accessTokenId, bool enableTracking);
}
