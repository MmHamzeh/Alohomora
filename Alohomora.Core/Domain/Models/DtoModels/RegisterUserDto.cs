using System.ComponentModel.DataAnnotations;

namespace Alohomora.Core.Domain.Models.DtoModels;

/// <summary>
/// Data transfer object for user registration with password
/// </summary>
public class RegisterUserDto : IDto
{
    /// <summary>
    /// User's phone number (required)
    /// </summary>
    [Required(ErrorMessage = "Phone number is required")]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// User's email address (optional)
    /// </summary>
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    public string? Email { get; set; }

    /// <summary>
    /// User's password (required for password-based login)
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// User's full name (optional)
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Remember me option for token expiration
    /// </summary>
    public bool RememberMe { get; set; }

    /// <summary>
    /// Return URL after successful registration
    /// </summary>
    public string? ReturnUrl { get; set; }
}
