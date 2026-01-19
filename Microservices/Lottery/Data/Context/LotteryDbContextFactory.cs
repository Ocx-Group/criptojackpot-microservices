using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CryptoJackpot.Lottery.Data.Context;

/// <summary>
/// Design-time factory for LotteryDbContext.
/// Used by Entity Framework Tools for migrations.
/// </summary>
public class LotteryDbContextFactory : IDesignTimeDbContextFactory<LotteryDbContext>
{
    public LotteryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LotteryDbContext>();
        
        // Use the default connection string for migrations
        // This matches the local development environment
        var connectionString = "Host=localhost;Port=5433;Database=cryptojackpot_lottery_db;Username=postgres;Password=postgres;";
        
        optionsBuilder.UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention();

        return new LotteryDbContext(optionsBuilder.Options);
    }
}
