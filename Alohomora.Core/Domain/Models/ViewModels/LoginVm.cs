namespace Alohomora.Core.Domain.Models.ViewModels;

public record LoginVm : IVm
{
    public LoginVm()
    {
    }

    public LoginVm(string accessToken, string refreshToken, string? returnUrl)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ReturnUrl = returnUrl;
    }

    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string? ReturnUrl { get; set; } = string.Empty;
}