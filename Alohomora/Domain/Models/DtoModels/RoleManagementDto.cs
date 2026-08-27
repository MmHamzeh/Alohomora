using System.ComponentModel.DataAnnotations;

namespace Alohomora.Core.Domain.Models.DtoModels;

/// <summary>
/// Data transfer object for role management operations
/// </summary>
public class CreateRoleDto : IDto
{
    /// <summary>
    /// Role name (unique identifier)
    /// </summary>
    [Required(ErrorMessage = "Role name is required")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Role display name in Persian/Farsi
    /// </summary>
    [Required(ErrorMessage = "Role Persian name is required")]
    public string FaName { get; set; } = string.Empty;

    /// <summary>
    /// Role description (optional)
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Data transfer object for assigning role to user
/// </summary>
public class AssignRoleToUserDto : IDto
{
    /// <summary>
    /// User's public ID
    /// </summary>
    [Required(ErrorMessage = "User ID is required")]
    public Guid UserId { get; set; }

    /// <summary>
    /// Role name to assign
    /// </summary>
    [Required(ErrorMessage = "Role name is required")]
    public string RoleName { get; set; } = string.Empty;
}

/// <summary>
/// Data transfer object for removing role from user
/// </summary>
public class RemoveRoleFromUserDto : IDto
{
    /// <summary>
    /// User's public ID
    /// </summary>
    [Required(ErrorMessage = "User ID is required")]
    public Guid UserId { get; set; }

    /// <summary>
    /// Role name to remove
    /// </summary>
    [Required(ErrorMessage = "Role name is required")]
    public string RoleName { get; set; } = string.Empty;
}
