using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Alohomora.Sql.Database;

/// <summary>
/// Design-time factory for creating DatabaseContext instances for EF Core migrations
/// </summary>
public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
        
        // Use the default connection string for migrations
        var connectionString = DatabaseContextHelper.ConnectionString;
        
        if (string.IsNullOrEmpty(connectionString))
        {
            // Fallback connection string for migration tooling
            connectionString = "Data Source=.;Initial Catalog=AlohomoraDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";
        }

        optionsBuilder.UseSqlServer(connectionString);

        return new DatabaseContext(optionsBuilder.Options);
    }
}
