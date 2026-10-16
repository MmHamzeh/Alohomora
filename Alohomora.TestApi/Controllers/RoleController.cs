namespace Alohomora.TestApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RoleController(IRoleService roleService) : ControllerBase
{
    /// <summary>
    /// Create a new role (requires Admin role)
    /// </summary>
    [HttpPost("create")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto, CancellationToken ct)
    {
        var result = await roleService.CreateRoleAsync(dto, ct);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Assign role to user (requires Admin role)
    /// </summary>
    [HttpPost("assign")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> AssignRoleToUser([FromBody] AssignRoleToUserDto dto, CancellationToken ct)
    {
        var result = await roleService.AssignRoleToUserAsync(dto, ct);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Remove role from user (requires Admin role)
    /// </summary>
    [HttpPost("remove")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> RemoveRoleFromUser([FromBody] RemoveRoleFromUserDto dto, CancellationToken ct)
    {
        var result = await roleService.RemoveRoleFromUserAsync(dto, ct);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get user roles
    /// </summary>
    [HttpGet("{userId}")]
    [Authorize]
    public async Task<IActionResult> GetUserRoles(Guid userId, CancellationToken ct)
    {
        var result = await roleService.GetUserRolesAsync(userId, ct);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
