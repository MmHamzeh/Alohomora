namespace Alohomora.Core.Common.Enums;

public enum AuthenticationType
{
    /// <summary>
    /// No authentication required. Anyone can access.
    /// </summary>
    AllowAnonymous = 0,

    /// <summary>
    /// User must be authenticated (signed in), but no role or permission checks.
    /// </summary>
    SignedIn = 1,

    /// <summary>
    /// User must have at least one of the required roles.
    /// </summary>
    CheckRoles = 2,

    /// <summary>
    /// User must have at least one of the required permissions.
    /// </summary>
    CheckPermissions = 3,

    /// <summary>
    /// User must have full permission checks (fine-grained authorization).
    /// </summary>
    FullCheck = 4,

}
