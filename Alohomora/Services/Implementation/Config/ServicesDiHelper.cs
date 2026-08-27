using Alohomora.Core.Services.Contact;

namespace Alohomora.Core.Services.Implementation.Config;

public static class ServicesDiHelper
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<TokenHelper>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IIdentityService, IdentityService>();

    }

}
