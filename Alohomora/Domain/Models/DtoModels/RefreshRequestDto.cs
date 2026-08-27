namespace Alohomora.Core.Domain.Models.DtoModels;

public class RefreshRequestDto
{
    internal string AccessToken { get; set; }
    internal string RefreshToken { get; set; }
}