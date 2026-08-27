using Microsoft.AspNetCore.Authorization;

namespace Alohomora.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class AlohomoraAuthAttribute : AuthorizeAttribute
{
    private AuthenticationType Type { get; }
    private string[]? RoleList { get; }


    public AlohomoraAuthAttribute(AuthenticationType type)
    {
        if (type == AuthenticationType.CheckRoles)
            throw new ArgumentException("For CheckRoles, use the constructor that accepts roles");

        Type = type;
    }

    public AlohomoraAuthAttribute(string[] roles)
    {
        Type = AuthenticationType.CheckRoles;
        RoleList = roles;
        Roles = string.Join(",", roles);
    }

}
