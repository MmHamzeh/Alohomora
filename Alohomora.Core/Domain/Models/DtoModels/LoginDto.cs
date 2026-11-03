namespace Alohomora.Core.Domain.Models.DtoModels;

public class LoginDto : IDto
{
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
    
}

public class PhoneNumberLoginPasswordDto : LoginDto, IPhoneNumberLogin, ILoginWithPassword
{
    public string PhoneNumber { get; set; } 
    public string Password { get; set; }
}

public class PhoneNumberLoginOtpDto : LoginDto, IPhoneNumberLogin, ILoginWithOtp
{
    public string PhoneNumber { get; set; } 
    public string AuthOtpCode { get; set; }
}



public class EmailLoginPasswordDto : LoginDto, IEmailLogin, ILoginWithPassword
{
    public string Email { get; set; } 
    public string Password { get; set; }
}

public class EmailLoginOtpDto : LoginDto, IEmailLogin, ILoginWithOtp
{
    public string Email { get; set; } 
    public string AuthOtpCode { get; set; } 
}



public class UsernameLoginPasswordDto : LoginDto, IEmailLogin, ILoginWithPassword
{
    public string Username { get; set; } 
    public string Password { get; set; } 
}

public class UsernameLoginOtpDto : LoginDto, IEmailLogin, ILoginWithOtp
{
    public string Username { get; set; } 
    public string AuthOtpCode { get; set; } 
}



public interface IPhoneNumberLogin
{
    public string PhoneNumber { get; set; }
}

public interface IEmailLogin
{
    public string Email { get; set; }
}

public interface IUsernameLogin
{
    public string Username { get; set; }
}

public interface ILoginWithOtp
{
    public string AuthOtpCode { get; set; }
}

public interface ILoginWithPassword
{
    public string Password { get; set; }
}