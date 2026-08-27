using Microsoft.Extensions.Configuration;

namespace Alohomora.Sql.Config;

internal static class DataAccessDiHelper
{
    internal static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? DatabaseContextHelper.ConnectionString;

        //DatabaseContext
        services.AddDbContextPool<DatabaseContext>(opt =>
            opt.UseSqlServer(connectionString));
        services.AddDbContextPool<DatabaseContextRead>(opt =>
            opt.UseSqlServer(connectionString));


        //Repositories

        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IAuthOtpRepository, AuthOtpRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        
        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}
