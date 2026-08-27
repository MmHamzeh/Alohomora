using System.Security.Claims;
using Alohomora.Core.Services.Contact;

namespace Alohomora.Core.Services.Implementation;

/// <summary>
/// Service for managing user-related operations and retrieving current user information from JWT claims
/// </summary>
public class UserService : IUserService
{
    #region Fields and Ctor

    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserService"/> class
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor</param>
    public UserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    #endregion

    /// <summary>
    /// Gets the current authenticated user's public ID from the JWT token claims
    /// </summary>
    /// <returns>The user's public ID as a Guid, or Guid.Empty if not authenticated</returns>
    public Guid CurrentUserId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
                return Guid.Empty;

            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier) 
                              ?? httpContext.User.FindFirst("sub");
            
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
                return Guid.Empty;

            return Guid.TryParse(userIdClaim.Value, out var userId) ? userId : Guid.Empty;
        }
    }
}