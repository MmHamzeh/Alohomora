using Alohomora.Core.Common.Configs;
using Alohomora.Core.Services.Contact.AuthServices;
using Alohomora.Core.Services.Contact.ExternalServices;
using Alohomora.Core.Services.Implementation;
using Alohomora.Core.Services.Implementation.Config;
using Alohomora.Core.Services.Implementation.ExternalServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Alohomora.Core;

public static class AlohomoraExtensions
{
    public static IServiceCollection AddAlohomora(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddSingleton<ITextSanitizerService, TextSanitizerService>();
        services.AddSingleton<ISmsService, PayamResanService>();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddAuthorization();
        services.AddSingleton<IAuthorizationPolicyProvider, AlohomoraAuthorizationPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, AppAuthorizationHandler>();

        ServicesDiHelper
            .ConfigureServices(services);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                using var serviceProvider = services.BuildServiceProvider();
                var tokenHelper = serviceProvider.GetRequiredService<TokenHelper>();
                options.TokenValidationParameters = tokenHelper.TokenValidationParameters;
            });

        return services;
    }

    public static IApplicationBuilder UseAlohomora(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }

}
