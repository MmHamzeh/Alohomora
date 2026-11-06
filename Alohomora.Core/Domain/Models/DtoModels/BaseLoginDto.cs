namespace Alohomora.Core.Domain.Models.DtoModels;

public abstract class BaseLoginDto : IDto
{
    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }

}
public abstract class BasePasswordLoginDto : BaseLoginDto, ILoginWithPassword
{
    public string? Password { get; set; }
}
public abstract class BaseOtpLoginDto : BaseLoginDto, ILoginWithOtp
{
    public string? AuthOtpCode { get; set; }
}

public sealed class PhoneNumberLoginPasswordDto : BasePasswordLoginDto, IPhoneNumberLogin
{
    public required string PhoneNumber { get; set; }
}

public sealed class PhoneNumberLoginOtpDto : BaseOtpLoginDto, IPhoneNumberLogin
{
    public required string PhoneNumber { get; set; }
}



public sealed class EmailLoginPasswordDto : BasePasswordLoginDto, IEmailLogin
{
    public required string Email { get; set; }
}

public sealed class EmailLoginOtpDto : BaseOtpLoginDto, IEmailLogin
{
    public required string Email { get; set; }
}



public sealed class UsernameLoginPasswordDto : BasePasswordLoginDto, IUsernameLogin
{
    public required string Username { get; set; }
}

public sealed class UsernameLoginOtpDto : BaseOtpLoginDto, IUsernameLogin
{
    public required string Username { get; set; }
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