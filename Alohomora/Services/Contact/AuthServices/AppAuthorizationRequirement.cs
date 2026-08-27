using Alohomora.Core.Common.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Alohomora.Core.Services.Contact.AuthServices;

public class AppAuthorizationRequirement : IAuthorizationRequirement
{

    internal AppAuthorizationRequirement(AuthenticationType type, IEnumerable<string>? roles = null)
    {
        Type = type;
        Roles = roles?.ToArray();
    }

    public AuthenticationType Type { get; }
    public IReadOnlyList<string>? Roles { get; }


}
