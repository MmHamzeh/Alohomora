namespace Alohomora.Services.Implementation;

internal class TokenService : ITokenService
{
    #region Fields and Ctor

    private readonly TokenHelper _tokenHelper;
    private readonly IUnitOfWork _unitOfWork;

    private readonly IRoleRepository _roleRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;

    internal TokenService(TokenHelper tokenHelper, IUnitOfWork unitOfWork)
    {
        _tokenHelper = tokenHelper;
        _unitOfWork = unitOfWork;

        _roleRepository = unitOfWork.RoleRepository;
        _refreshTokenRepository = unitOfWork.RefreshTokenRepository;
        _userRepository = unitOfWork.UserRepository;
    }

    #endregion

    public async Task<CreateTokenResult> GenerateTokensAsync(User user, bool rememberMe = false)
    {
        var roles = await _roleRepository.GetUserRolesName(user.Id);
        var accessTokenJwt = _tokenHelper.CreateAccessTokenAsync(user.PublicId, roles);

        var accessTokenId = TokenHelper.GetAccessTokenId(accessTokenJwt);

        var refreshToken = TokenHelper.CreateRefreshToken(user.Id, accessTokenId.Value, rememberMe);

        await _refreshTokenRepository.AddAsync(refreshToken);
        _ = await _unitOfWork.SaveChanges();

        var accessToken = _tokenHelper.WriteToken(accessTokenJwt);

        return new CreateTokenResult()
        {
            AccessToken = accessToken, AccessTokenId = accessTokenId.Value, RefreshToken = refreshToken.Token
        };
    }

    public async Task<CreateTokenResult> RefreshTokensAsync(string accessToken, string refreshToken)
    {
        var accessTokenId = _tokenHelper.GetAccessTokenId(accessToken);

        // اعتبارسنجی توکن بازنشانی
        var oldRefreshToken = await _refreshTokenRepository.GetByTokenAccessTokenId(refreshToken, accessTokenId.Value, enableTracking: true);

        if (oldRefreshToken == null || oldRefreshToken.IsRevoked || oldRefreshToken.Expires < DateTime.Now)
            throw new SecurityTokenException("Invalid refresh token");

        // علامت‌گذاری به عنوان Revoked
        oldRefreshToken.IsRevoked = true;
        await _unitOfWork.SaveChanges();

        var user = await _userRepository.GetById(oldRefreshToken.Id, enableTracking: false);

        if (user is null)
            throw new SecurityTokenException("user is null");

        return await GenerateTokensAsync(user, oldRefreshToken.RememberMe);
    }
}