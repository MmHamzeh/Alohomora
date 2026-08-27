namespace Alohomora.Core.Domain.Models.DtoModels;

public class LoginDto : IDto
{
    internal string PhoneNumber { get; set; }
    internal bool RememberMe { get; set; }

    internal string? ReturnUrl { get; set; }

}

public class LoginPasswordDto : LoginDto
{
    internal string Password { get; set; }
}

public class LoginOtpDto : LoginDto
{
    internal string AuthOtpCode { get; set; }

}