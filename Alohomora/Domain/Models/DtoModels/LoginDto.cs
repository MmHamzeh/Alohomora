namespace Alohomora.Domain.Models.DtoModels;

internal class LoginDto : IDto
{
    internal string PhoneNumber { get; set; }
    internal bool RememberMe { get; set; }

    internal string? ReturnUrl { get; set; }

}

internal class LoginPasswordDto : LoginDto
{
    internal string Password { get; set; }
}

internal class LoginOtpDto : LoginDto
{
    internal string AuthOtpCode { get; set; }

}