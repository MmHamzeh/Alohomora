using Alohomora.Services.Implementation.ExternalServices;

namespace Alohomora.Services.Implementation.Config;

public static class ServicesDiHelper
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ITextSanitizerService, TextSanitizerService>();
        services.AddSingleton<ISmsService, PayamResanService>();

        services.AddSingleton<TokenHelper>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IIdentityService, IdentityService>();

    }

}
