namespace Alohomora.Domain.Models.DtoModels;

internal class RefreshRequestDto
{
    internal string AccessToken { get; set; }
    internal string RefreshToken { get; set; }
}