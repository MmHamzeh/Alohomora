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
using Microsoft.Extensions.DependencyInjection;

namespace Alohomora.Core;

public static class AlohomoraExtensions
{
    public static IServiceCollection AddAlohomora(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddSingleton<ITextSanitizerService, TextSanitizerService>();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddAuthorization();
        services.AddSingleton<IAuthorizationPolicyProvider, AlohomoraAuthorizationPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, AppAuthorizationHandler>();

        ServicesDiHelper
            .ConfigureServices(services);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            using var serviceProvider = services.BuildServiceProvider();
            var tokenHelper = serviceProvider.GetRequiredService<TokenHelper>();
            options.TokenValidationParameters = tokenHelper.TokenValidationParameters;
            
            // Configure JWT Bearer events
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine("🔴 Authentication failed: " + context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    Console.WriteLine("🟠 Challenge: " + context.ErrorDescription);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    Console.WriteLine("🟢 Token validated successfully");
                    return Task.CompletedTask;
                },
                OnMessageReceived = context =>
                {
                    // Check for token in query string or body if not in header
                    var accessToken = context.Request.Query["access_token"];
                    
                    // If there's no token in the header, try to get it from the query string
                    if (string.IsNullOrEmpty(context.Request.Headers["Authorization"]) && !string.IsNullOrEmpty(accessToken))
                    {
                        context.Token = accessToken;
                    }
                    
                    return Task.CompletedTask;
                },
                OnForbidden = context =>
                {
                    Console.WriteLine("🟠 Forbidden: " + context.Result.Succeeded);
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }

    private static IServiceCollection AddEasyCaching(IServiceCollection services, IConfiguration configuration)
    {
        //TODO: get this data from configuration
        var redisServerAddress = EasyCachingConfigs.AccessTokenIdStoreHost;
        var redisServerPort = EasyCachingConfigs.AccessTokenIdStorePort;

        var accessTokenIdStoreName = EasyCachingConfigs.AccessTokenIdStoreName;

        services.AddEasyCaching(options => 
        {
            options.UseInMemory(AccessTokenIdStoreName);

            // options.UseRedis(config => 
            // {
            //     config.DBConfig.Endpoints.Add(new ServerEndPoint(redisServerAddress, redisServerPort));
            // }, AccessTokenIdStoreName)
            // .WithMessagePack();            
        });    
    }

    public static IApplicationBuilder UseAlohomora(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }

}
