
using Alohomora.Core;
using Alohomora.Sql;
using Alohomora.Sql.Config;
using Alohomora.Sql.Database;
using Microsoft.EntityFrameworkCore;

namespace Alohomora.TestApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        
        // Add Alohomora authentication and authorization
        builder.Services.AddAlohomora(builder.Configuration);
        
        // Add Alohomora SQL Server data access layer
        builder.Services.AddAlohomoraSql(builder.Configuration);
        
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        // Use Alohomora authentication and authorization middleware
        app.UseAlohomora();

        app.MapControllers();

        app.Run();
    }
}
