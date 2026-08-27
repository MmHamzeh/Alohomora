namespace Alohomora.DataAccess.Implementation.Database;

internal class DatabaseContext : DbContext
{
    #region Ctor

    protected DatabaseContext()
    {
        
    }

    internal DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
        
    }

    #endregion

    #region DbSet

    internal DbSet<RefreshToken> RefreshTokens { get; set; }
    internal DbSet<AuthOtp> AuthOtps { get; set; }
    internal DbSet<User> Users { get; set; }
    internal DbSet<UserRole> UserRoles { get; set; }
    internal DbSet<Role> Roles { get; set; }
    internal DbSet<UserStatus> UserStatuses { get; set; }

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.UseCollation(DatabaseContextHelper.Collation);

        DatabaseContextHelper.EnableIsDeletedQueryFilter(modelBuilder);
        DatabaseContextHelper.ConfigureEntities(modelBuilder);
        DatabaseContextHelper.SeedValues(modelBuilder);

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;

        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseSqlServer(DatabaseContextHelper.ConnectionString);
        optionsBuilder.EnableDetailedErrors(DatabaseContextHelper.EnableDetailedErrors);
        optionsBuilder.EnableSensitiveDataLogging(DatabaseContextHelper.EnableSensitiveDataLogging);

    }

}

internal sealed class DatabaseContextRead : DatabaseContext
{
    #region Ctor

    internal DatabaseContextRead(DbContextOptions<DatabaseContextRead> options)
    {
        ChangeTracker.AutoDetectChangesEnabled = false;
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    #endregion
}