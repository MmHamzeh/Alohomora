using Alohomora.Core.DataAccess.Contract;
using Alohomora.Core.DataAccess.Contract.Repositories;
using Alohomora.Core.Domain.Models.DbModels;
using Alohomora.Core.Domain.Models.DtoModels;
using Alohomora.Core.Services.Contact;

namespace Alohomora.Core.Services.Implementation;

public class TokenService : ITokenService
{
    #region Fields and Ctor

    private readonly TokenHelper _tokenHelper;
    private readonly IUnitOfWork _unitOfWork;

    private readonly IRoleRepository _roleRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly TimeProvider _timeProvider;

    internal TokenService(TokenHelper tokenHelper, IUnitOfWork unitOfWork, TimeProvider timeProvider)
    {
        _tokenHelper = tokenHelper;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;

        _roleRepository = unitOfWork.RoleRepository;
        _refreshTokenRepository = unitOfWork.RefreshTokenRepository;
        _userRepository = unitOfWork.UserRepository;
    }

    #endregion

    public async Task<CreateTokenResult> GenerateTokensAsync(User user, bool rememberMe = false)
    {
        var roles = await _roleRepository.GetUserRolesName(user.Id);
        var accessTokenJwt = _tokenHelper.CreateAccessToken(user.PublicId, roles);

        var accessTokenId = TokenHelper.GetAccessTokenId(accessTokenJwt);

        var refreshToken = TokenHelper.CreateRefreshToken(user.Id, accessTokenId.Value, _timeProvider, rememberMe);

        await _refreshTokenRepository.AddAsync(refreshToken);
        _ = await _unitOfWork.SaveChanges();

        var accessToken = _tokenHelper.WriteToken(accessTokenJwt);

        return new CreateTokenResult()
        {
            AccessToken = accessToken, AccessTokenId = accessTokenId.Value, RefreshToken = refreshToken.Token
        };
    }

    public async Task<CreateTokenResult> RefreshTokensAsync(string accessToken, string refreshToken, CancellationToken ct)
    {
        var accessTokenId = _tokenHelper.GetAccessTokenId(accessToken);

        // اعتبارسنجی توکن بازنشانی
        var oldRefreshToken = await _refreshTokenRepository.GetByTokenAccessTokenId(refreshToken, accessTokenId.Value, enableTracking: true);

        if (oldRefreshToken == null || oldRefreshToken.IsRevoked || oldRefreshToken.Expires < _timeProvider.GetUtcNow())
            throw new SecurityTokenException("Invalid refresh token");

        // علامت‌گذاری به عنوان Revoked
        oldRefreshToken.IsRevoked = true;
        await _unitOfWork.SaveChanges();

        var user = await _userRepository.GetByIdAsync(oldRefreshToken.UserId, enableTracking: false, ct);

        if (user is null)
            throw new SecurityTokenException("user is null");

        return await GenerateTokensAsync(user, oldRefreshToken.RememberMe);
    }
}
