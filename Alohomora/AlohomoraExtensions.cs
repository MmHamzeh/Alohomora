using Alohomora.Core.Services.Contact.ExternalServices;
using Alohomora.Core.Services.Implementation.Config;
using Alohomora.Core.Services.Implementation.ExternalServices;
using Microsoft.AspNetCore.Builder;

namespace Alohomora.Core;

public static class AlohomoraExtensions
{
    public static IServiceCollection AddAlohomora(this IServiceCollection services)
    {
        services.AddSingleton<ITextSanitizerService, TextSanitizerService>();
        services.AddSingleton<ISmsService, PayamResanService>();

        ServicesDiHelper
            .ConfigureServices(services);

        return services;
    }

    public static IApplicationBuilder UseAlohomora(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        //app.UseAuthorization();


        return app;
    }

}
