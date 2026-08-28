using Alohomora.PayamResan.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Alohomora.PayamResan;

public static class AlohomoraPayamResanExtensions
{
    public static IServiceCollection AddAlohomoraPayamResan(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ISmsService, PayamResanService>();

        return services;
    }
    
}
