using System.ComponentModel.DataAnnotations;

namespace Alohomora.Core.Domain.Models.DtoModels;

/// <summary>
/// Data transfer object for password reset request
/// </summary>
public class ForgotPasswordDto : IDto
{
    /// <summary>
    /// User's phone number to send reset OTP
    /// </summary>
    [Required(ErrorMessage = "Phone number is required")]
    public string PhoneNumber { get; set; } = string.Empty;
}

/// <summary>
/// Data transfer object for password reset confirmation
/// </summary>
public class ResetPasswordDto : IDto
{
    /// <summary>
    /// User's phone number
    /// </summary>
    [Required(ErrorMessage = "Phone number is required")]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// OTP code received via SMS
    /// </summary>
    [Required(ErrorMessage = "OTP code is required")]
    public string OtpCode { get; set; } = string.Empty;

    /// <summary>
    /// New password
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string NewPassword { get; set; } = string.Empty;
}
