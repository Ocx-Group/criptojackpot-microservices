using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CryptoJackpot.Winner.Data.Context;

/// <summary>
/// Design-time factory for WinnerDbContext.
/// Used by Entity Framework Tools for migrations.
/// </summary>
public class WinnerDbContextFactory : IDesignTimeDbContextFactory<WinnerDbContext>
{
    public WinnerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WinnerDbContext>();

        // Use the default connection string for migrations
        // This matches the local development environment
        var connectionString = "Host=localhost;Port=5433;Database=cryptojackpot_winner_db;Username=postgres;Password=postgres;";

        optionsBuilder.UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention();

        return new WinnerDbContext(optionsBuilder.Options);
    }
}
