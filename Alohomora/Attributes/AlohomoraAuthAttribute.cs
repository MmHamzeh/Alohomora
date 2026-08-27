using Alohomora.Core.Common.Enums;
using Alohomora.Core.Services.Contact.AuthServices;
using Microsoft.AspNetCore.Authorization;

namespace Alohomora.Core.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class AlohomoraAuthAttribute : AuthorizeAttribute
{
    internal AuthenticationType Type { get; }
    private string[]? RoleList { get; }


    public AlohomoraAuthAttribute(AuthenticationType type)
    {
        if (type == AuthenticationType.CheckRoles)
            throw new ArgumentException("For CheckRoles, use the constructor that accepts roles");

        Type = type;
        Policy = AlohomoraPolicy.BuildPolicyName(type, roles: null);
    }

    public AlohomoraAuthAttribute(string[] roles)
    {
        Type = AuthenticationType.CheckRoles;
        RoleList = roles;
        Roles = string.Join(",", roles);
        Policy = AlohomoraPolicy.BuildPolicyName(Type, roles);
    }

}
