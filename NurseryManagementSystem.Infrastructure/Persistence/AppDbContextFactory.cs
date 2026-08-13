using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NurseryManagementSystem.Infrastructure.Persistence
{
    /// <summary>
    /// Enables "dotnet ef" tooling (migrations/scaffolding) to create the context
    /// at design time without spinning up the API host. Override the connection
    /// string with the NMS_CONNECTION_STRING environment variable when needed.
    /// </summary>
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var connectionString =
                Environment.GetEnvironmentVariable("NMS_CONNECTION_STRING")
                ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? "Host=localhost;Port=5432;Database=nursery_management;Username=postgres;Password=liojinx12_kiro.";

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
