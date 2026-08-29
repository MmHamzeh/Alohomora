using Alohomora.Core.Services.Contact.ExternalServices;
using Alohomora.PayamResan.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Alohomora.PayamResan;

public static class AlohomoraPayamResanExtensions
{
    public static IServiceCollection AddAlohomoraPayamResan(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient("PayamResasnV3", httpClient =>
        {
            httpClient.BaseAddress = new Uri("http://api.sms-webservice.com/api/V3/");
        });

        //TODO: Get this from configuration
        var payamResanApiKey = "";
        var sender = 0L;

        //services.AddSingleton<ISmsService, PayamResanV3Service>();

        services.AddSingleton<ISmsService, PayamResanV3Service>(sp =>
        {
            var httpClientFactoryService = sp.GetRequiredService<IHttpClientFactory>();
            
            return new PayamResanV3Service(httpClientFactoryService, payamResanApiKey, sender);
        });

        return services;
    }
    
}
