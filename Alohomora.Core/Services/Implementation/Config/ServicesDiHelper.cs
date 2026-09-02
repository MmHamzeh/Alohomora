using Alohomora.Core.Services.Contact;
using Alohomora.Core.Services.Contact.ExternalServices;

namespace Alohomora.Core.Services.Implementation.Config;

public static class ServicesDiHelper
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<TokenHelper>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IIdentityService, IdentityService>();

        // Default email service (no-op). Consumers may override with a real implementation in DI.
        services.AddSingleton<IEmailService, NoOpEmailService>();
    }

}
