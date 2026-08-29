namespace Alohomora.Core.Domain.Models.ViewModels;

public class LoginVm : IVm
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string? ReturnUrl { get; set; } = string.Empty;
}