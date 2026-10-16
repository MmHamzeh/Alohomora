namespace Alohomora.TestApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController(IUserService userService) : ControllerBase
{
    /// <summary>
    /// Get current user info
    /// </summary>
    [HttpGet("CurrentUserId")]
    [Authorize]
    public IActionResult GetCurrentUserInfo()
    {
        var currentUserId = userService.CurrentUserId ?? Guid.Empty;
        return Ok(new { UserId = currentUserId });
    }
}
