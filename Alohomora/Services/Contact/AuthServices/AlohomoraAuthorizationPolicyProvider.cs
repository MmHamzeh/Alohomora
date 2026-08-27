using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Alohomora.Core.Services.Contact.AuthServices;

internal class AlohomoraAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public AlohomoraAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
    {
    }

    public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (AlohomoraPolicy.TryParse(policyName, out var type, out var roles))
        {
            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new AppAuthorizationRequirement(type, roles))
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return base.GetPolicyAsync(policyName);
    }
}
