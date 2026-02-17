using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Comanda.Infrastructure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ComandaDbContext>
{
    public ComandaDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "Comanda.Api");
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ComandaDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=db.oifvislvtkqsxelyzgyi.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=YOUR_PASSWORD";
        optionsBuilder.UseNpgsql(connectionString);

        return new ComandaDbContext(optionsBuilder.Options);
    }
}
