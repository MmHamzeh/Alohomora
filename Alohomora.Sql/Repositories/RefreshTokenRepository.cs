namespace Alohomora.Sql.Repositories;

internal class RefreshTokenRepository : IRefreshTokenRepository
{
    #region Fields and Ctor

    private readonly DatabaseContext _dbContext;
    

    internal RefreshTokenRepository(DatabaseContext? dbContext = null)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        
    }

    #endregion

    public Task<RefreshToken?> GetActiveByUserPublicId(Guid userPublicId, bool enableTracking, CancellationToken ct)
    {
        var refreshTokens = enableTracking
            ? _dbContext.RefreshTokens
            : _dbContext.RefreshTokens.AsNoTracking();

        return refreshTokens.FirstOrDefaultAsync(e =>
            e.User.PublicId == userPublicId && e.IsRevoked == false && e.Expires > DateTime.Now, ct);
    }

    public async Task RevokeByAccessTokenId(Guid accessTokenId)
    {
        var refreshToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.AccessTokenId == accessTokenId);
        if (refreshToken != null)
            refreshToken.IsRevoked = true;
    }

    public async Task AddAsync(RefreshToken refreshToken)
    {
        await _dbContext.RefreshTokens.AddAsync(refreshToken);
    }

    public async Task<RefreshToken?> GetByTokenAccessTokenId(string token, Guid accessTokenId, bool enableTracking)
    {
        var refreshTokens = enableTracking
            ? _dbContext.RefreshTokens
            : _dbContext.RefreshTokens.AsNoTracking();

        return await refreshTokens.FirstOrDefaultAsync(e => e.Token == token && e.AccessTokenId == accessTokenId);

    }
}