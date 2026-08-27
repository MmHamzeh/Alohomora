using Alohomora.Core;
using Alohomora.Sql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Alohomora.TestApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IIdentityService _identityService;
    private readonly IUserService _userService;

    public AuthController(IIdentityService identityService, IUserService userService)
    {
        _identityService = identityService;
        _userService = userService;
    }

    /// <summary>
    /// Login with password
    /// </summary>
    [HttpPost("login-password")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginPassword([FromBody] LoginPasswordDto dto, CancellationToken ct)
    {
        var result = await _identityService.LoginPasswordAsync(dto, ct);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Send OTP for login
    /// </summary>
    [HttpPost("send-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> SendLoginOtp([FromBody] LoginDto dto, CancellationToken ct)
    {
        var result = await _identityService.SendLoginOtpAsync(dto, ct);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Confirm OTP login
    /// </summary>
    [HttpPost("confirm-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginOtpConfirm([FromBody] LoginOtpDto dto, CancellationToken ct)
    {
        var result = await _identityService.LoginOtpConfirmAsync(dto, ct);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshRequestDto dto)
    {
        var result = await _identityService.RefreshToken(dto);
        
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
        var result = await _identityService.LogOutAsync();
        return Ok(result);
    }

    /// <summary>
    /// Get current user info
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUserInfo()
    {
        var currentUserId = _userService.CurrentUserId;
        return Ok(new { UserId = currentUserId });
    }
}
