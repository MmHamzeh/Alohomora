using Alohomora.Sql.Config;

namespace Alohomora.Sql;

public static class AlohomoraSqlExtensions
{
    public static IServiceCollection AddAlohomora(this IServiceCollection services)
    {
        DataAccessDiHelper.ConfigureServices(services);

        return services;
    }
}