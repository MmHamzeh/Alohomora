namespace Alohomora.Core.Domain.Models.DtoModels;

public class LoginDto : IDto
{
    public string PhoneNumber { get; set; } = string.Empty;
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }

}

public class LoginPasswordDto : LoginDto
{
    public string Password { get; set; } = string.Empty;
}

public class LoginOtpDto : LoginDto
{
    public string AuthOtpCode { get; set; } = string.Empty;

}