namespace Alohomora.Services.Contact;

internal interface ITokenService
{
    Task<CreateTokenResult> GenerateTokensAsync(User user, bool rememberMe = false);
    Task<CreateTokenResult> RefreshTokensAsync(string accessToken, string refreshToken);

}