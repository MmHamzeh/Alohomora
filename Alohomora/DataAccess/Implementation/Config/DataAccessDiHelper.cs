using Alohomora.DataAccess.Implementation.Repositories;

namespace Alohomora.DataAccess.Implementation.Config;

internal static class DataAccessDiHelper
{
    internal static void ConfigureServices(IServiceCollection services)
    {

        //DatabaseContext
        services.AddDbContextPool<DatabaseContext>(opt =>
            opt.UseSqlServer(DatabaseContextHelper.ConnectionString));
        services.AddDbContextPool<DatabaseContextRead>(opt =>
            opt.UseSqlServer(DatabaseContextHelper.ConnectionString));


        //Repositories

        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IAuthOtpRepository, AuthOtpRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
    }
}
