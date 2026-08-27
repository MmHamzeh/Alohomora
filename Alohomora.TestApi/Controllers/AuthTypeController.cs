using Alohomora.Core;
using Alohomora.Core.Domain.Models.DtoModels;
using Alohomora.Core.Services.Contact;
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

    /// <summary>
    /// Register a new user with password
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDto dto, CancellationToken ct)
    {
        var result = await _identityService.RegisterUserAsync(dto, ct);

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
        var result = await _identityService.ForgotPasswordAsync(dto, ct);

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
        var result = await _identityService.ResetPasswordAsync(dto, ct);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Create a new role (requires Admin role)
    /// </summary>
    [HttpPost("roles/create")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto, CancellationToken ct)
    {
        var result = await _identityService.CreateRoleAsync(dto, ct);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Assign role to user (requires Admin role)
    /// </summary>
    [HttpPost("roles/assign")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> AssignRoleToUser([FromBody] AssignRoleToUserDto dto, CancellationToken ct)
    {
        var result = await _identityService.AssignRoleToUserAsync(dto, ct);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Remove role from user (requires Admin role)
    /// </summary>
    [HttpPost("roles/remove")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> RemoveRoleFromUser([FromBody] RemoveRoleFromUserDto dto, CancellationToken ct)
    {
        var result = await _identityService.RemoveRoleFromUserAsync(dto, ct);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get user roles
    /// </summary>
    [HttpGet("roles/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetUserRoles(Guid userId, CancellationToken ct)
    {
        var result = await _identityService.GetUserRolesAsync(userId, ct);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
