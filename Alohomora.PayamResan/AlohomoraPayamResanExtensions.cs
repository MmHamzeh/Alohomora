using Alohomora.Core.Services.Contact.ExternalServices;
using Alohomora.PayamResan.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Alohomora.PayamResan;

public static class AlohomoraPayamResanExtensions
{
    public static IServiceCollection AddAlohomoraPayamResan(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient("PayamResanV3", httpClient =>
        {
            httpClient.BaseAddress = new Uri("http://api.sms-webservice.com/api/V3/");
        });

        var payamResanApiKey = configuration.GetValue<string>("SmsService:PayamResan:ApiKey");
        var sender = configuration.GetValue<long>("SmsService:PayamResan:AuthSender");

        if (string.IsNullOrEmpty(payamResanApiKey))
            throw new Exception("SmsService > PayamResan > ApiKey is not provided");

        if (sender == 0L)
            throw new Exception("SmsService > PayamResan > AuthSender is not provided");

        services.AddSingleton<ISmsService, PayamResanService>(sp =>
        {
            var httpClientFactoryService = sp.GetRequiredService<IHttpClientFactory>();
            return new PayamResanService(httpClientFactoryService, payamResanApiKey, sender);
        });

        return services;
    }

}
