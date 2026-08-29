namespace Alohomora.Core.Domain.Models.DtoModels;

public class CreateTokenResult
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public Guid AccessTokenId { get; set; }
}