using Microsoft.AspNetCore.Authorization;

namespace Alohomora.Services.Contact.AuthServices;

internal class AppAuthorizationRequirement : IAuthorizationRequirement
{

    internal AppAuthorizationRequirement(AuthenticationType type, IEnumerable<string>? roles = null)
    {
        Type = type;
        Roles = roles?.ToArray();
    }

    public AuthenticationType Type { get; }
    public IReadOnlyList<string>? Roles { get; }


}
