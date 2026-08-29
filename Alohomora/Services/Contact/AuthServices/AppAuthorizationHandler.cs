using Alohomora.Core.Common.Enums;
using Alohomora.Core.Services.Implementation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Net.Http.Headers;

namespace Alohomora.Core.Services.Contact.AuthServices;

public class AppAuthorizationHandler(TokenHelper tokenHelper) : AuthorizationHandler<AppAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AppAuthorizationRequirement requirement)
    {
        if (requirement == null)
            context.Succeed(requirement);

        switch (requirement.Type)
        {
            case AuthenticationType.AllowAnonymous:
                return HandleAllowAnonymous(context, requirement);

            case AuthenticationType.SignedIn:
                return HandleSignedIn(context, requirement);

            case AuthenticationType.CheckRoles:
                return HandleCheckRoles(context, requirement);

            case AuthenticationType.CheckPermissions:
                return HandleCheckToken(context, requirement);

            case AuthenticationType.FullCheck:
                return HandleCheckRoles(context, requirement);

            default:
                context.Fail();
                return Task.CompletedTask;
        }
    }


    private static Task HandleAllowAnonymous(
        AuthorizationHandlerContext context,
        AppAuthorizationRequirement requirement)
    {
        context.Succeed(requirement);
        return Task.CompletedTask;
    }

    private async Task HandleSignedIn(
        AuthorizationHandlerContext context,
        AppAuthorizationRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated == true)
            await HandleCheckToken(context, requirement);
        else
            context.Fail();
    }

    private static Task HandleCheckRoles(
        AuthorizationHandlerContext context,
        AppAuthorizationRequirement requirement)
    {


        if (requirement.Roles == null || requirement.Roles.Count == 0)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        bool hasRole = requirement.Roles
            .Any(role => context.User.IsInRole(role));

        if (hasRole)
            context.Succeed(requirement);
        else
            context.Fail();

        return Task.CompletedTask;
    }

    private static Task HandleCheckUser(
        AuthorizationHandlerContext context,
        AppAuthorizationRequirement requirement)
    {
        // TODO: ensure user exists, is active, not banned, etc.
        context.Succeed(requirement);
        return Task.CompletedTask;
    }


    private async Task HandleCheckToken(
    AuthorizationHandlerContext context,
    AppAuthorizationRequirement requirement)
    {
        var httpContext = (context.Resource as DefaultHttpContext)
            ?? throw new InvalidOperationException("HttpContext not available");

        var authHeader = httpContext.Request.Headers[HeaderNames.Authorization].ToString() ?? string.Empty;

        var isValid = await tokenHelper.ValidateToken(authHeader);

        if (isValid)
            context.Succeed(requirement);
        else
            context.Fail();
    }
}
