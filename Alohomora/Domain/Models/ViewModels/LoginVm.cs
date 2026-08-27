namespace Alohomora.Domain.Models.ViewModels;

internal class LoginVm : IVm
{
    internal string AccessToken { get; set; }
    internal string RefreshToken { get; set; }
    internal string ReturnUrl { get; set; } = string.Empty;
}