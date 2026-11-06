namespace Alohomora.TestApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IIdentityService identityService) : ControllerBase
{

    #region Login

    #region Passwor


    /// <summary>
    /// Login with password
    /// </summary>
    [HttpPost("login-phoneNumber-password")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginPassword([FromBody] PhoneNumberLoginPasswordDto dto, CancellationToken ct)
    {
        var result = await identityService.LoginPasswordAsync(dto, ct);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }


    /// <summary>
    /// Login with password
    /// </summary>
    [HttpPost("login-email-password")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginPassword([FromBody] EmailLoginPasswordDto dto, CancellationToken ct)
    {
        var result = await identityService.LoginPasswordAsync(dto, ct);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }


    /// <summary>
    /// Login with password
    /// </summary>
    [HttpPost("login-username-password")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginPassword([FromBody] UsernameLoginPasswordDto dto, CancellationToken ct)
    {
        var result = await identityService.LoginPasswordAsync(dto, ct);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }





    #endregion

    #endregion


    /// <summary>
    /// Send OTP for login
    /// </summary>
    [HttpPost("send-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> SendLoginOtp([FromBody] PhoneNumberLoginOtpDto dto, CancellationToken ct)
    {
        var result = await identityService.SendLoginOtpAsync(dto, ct: ct);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Confirm OTP login
    /// </summary>
    [HttpPost("confirm-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginOtpConfirm([FromBody] PhoneNumberLoginOtpDto dto, CancellationToken ct)
    {
        var result = await identityService.LoginOtpConfirmAsync(dto, ct);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshRequestDto dto, CancellationToken ct)
    {
        var result = await identityService.RefreshToken(dto, ct);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Logout current user
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var result = await identityService.LogOutAsync();
        return Ok(result);
    }

    /// <summary>
    /// Register a new user with password
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDto dto, CancellationToken ct)
    {
        var result = await identityService.RegisterUserAsync(dto, ct);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Request password reset OTP
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto, CancellationToken ct)
    {
        var result = await identityService.ForgotPasswordAsync(dto, ct);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Reset password with OTP
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto, CancellationToken ct)
    {
        var result = await identityService.ResetPasswordAsync(dto, ct);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

}
