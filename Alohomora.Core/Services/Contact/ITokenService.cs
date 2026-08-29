using Alohomora.Core.Domain.Models.DbModels;
using Alohomora.Core.Domain.Models.DtoModels;

namespace Alohomora.Core.Services.Contact;

public interface ITokenService
{
    Task<CreateTokenResult> GenerateTokensAsync(User user, bool rememberMe = false);
    Task<CreateTokenResult> RefreshTokensAsync(string accessToken, string refreshToken, CancellationToken ct);

}