using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CryptoJackpot.Content.Data.Context;

/// <summary>
/// Design-time factory for ContentDbContext.
/// Used by Entity Framework Tools for migrations.
/// </summary>
public class ContentDbContextFactory : IDesignTimeDbContextFactory<ContentDbContext>
{
    public ContentDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ContentDbContext>();
        
        var connectionString = "Host=localhost;Port=5433;Database=cryptojackpot_content_db;Username=postgres;Password=postgres;";
        
        optionsBuilder.UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention();

        return new ContentDbContext(optionsBuilder.Options);
    }
}
