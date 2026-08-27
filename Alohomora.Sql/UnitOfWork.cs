namespace Alohomora.Sql;

internal class UnitOfWork : IUnitOfWork, IDisposable
{
    #region Fields And Ctor

    private readonly IServiceProvider _serviceProvider;
    private readonly DatabaseContext _dbContext;

    internal UnitOfWork(IServiceProvider serviceProvider, DatabaseContext dbContext)
    {
        _serviceProvider = serviceProvider;
        _dbContext = dbContext;
    }

    #endregion


    #region Repositories


    private IRefreshTokenRepository? _refreshTokenRepository = null;
    public IRefreshTokenRepository RefreshTokenRepository =>
        _refreshTokenRepository ??= _serviceProvider.GetRequiredService<IRefreshTokenRepository>();


    private IAuthOtpRepository? _authOtpRepository = null;
    public IAuthOtpRepository AuthOtpRepository =>
        _authOtpRepository ??= _serviceProvider.GetRequiredService<IAuthOtpRepository>();

    private IUserRepository? _userRepository = null;
    public IUserRepository UserRepository =>
        _userRepository ??= _serviceProvider.GetRequiredService<IUserRepository>();

    private IRoleRepository? _roleRepository = null;
    public IRoleRepository RoleRepository =>
        _roleRepository ??= _serviceProvider.GetRequiredService<IRoleRepository>();

    private IUserRoleRepository? _userRoleRepository = null;
    public IUserRoleRepository UserRoleRepository =>
        _userRoleRepository ??= _serviceProvider.GetRequiredService<IUserRoleRepository>();





    #endregion

    #region Methods

    public async Task<int> SaveChanges()
    {
        return await _dbContext.SaveChangesAsync();
    }

    public void RejectChanges()
    {
        foreach (var entity in _dbContext.ChangeTracker.Entries())
        {
            switch (entity.State)
            {
                case EntityState.Deleted:
                case EntityState.Modified:
                    entity.State = EntityState.Modified;
                    entity.State = EntityState.Unchanged;
                    break;
                case EntityState.Added:
                    entity.State = EntityState.Detached;
                    break;
            }
        }
    }

    public void Dispose()
    {
        var repositories = typeof(UnitOfWork).GetFields().OfType<IRepository?>().Where(e => e != null).ToList();
        repositories.ForEach(e => e = null);

        _dbContext.Dispose();

        GC.Collect();
        GC.SuppressFinalize(this);
    }

    #endregion
}

