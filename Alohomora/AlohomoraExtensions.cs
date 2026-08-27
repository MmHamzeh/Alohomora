using Microsoft.AspNetCore.Builder;

namespace Alohomora;

public static class AlohomoraExtensions
{
    public static IServiceCollection AddAlohomora(this IServiceCollection services)
    {
        Alohomora.DataAccess.Implementation.Config
            .DataAccessDiHelper
            .ConfigureServices(services);

        Alohomora.Services.Implementation.Config
            .ServicesDiHelper
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
