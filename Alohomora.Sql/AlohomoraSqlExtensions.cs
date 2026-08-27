using Alohomora.Sql.Config;
using Microsoft.Extensions.Configuration;

namespace Alohomora.Sql;

public static class AlohomoraSqlExtensions
{
    public static IServiceCollection AddAlohomoraSql(this IServiceCollection services, IConfiguration configuration)
    {
        DataAccessDiHelper.ConfigureServices(services, configuration);

        return services;
    }
}