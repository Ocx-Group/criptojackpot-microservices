using CryptoJackpot.Winner.Data.Context.Configurations;
using CryptoJackpot.Winner.Domain.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CryptoJackpot.Winner.Data.Context;

public class WinnerDbContext : DbContext
{
    public WinnerDbContext(DbContextOptions<WinnerDbContext> options) : base(options)
    {
    }

    public DbSet<LotteryWinner> Winners { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new LotteryWinnerConfiguration());

        // MassTransit Outbox configuration with snake_case naming
        modelBuilder.AddInboxStateEntity(x => x.ToTable("inbox_state"));
        modelBuilder.AddOutboxMessageEntity(x => x.ToTable("outbox_message"));
        modelBuilder.AddOutboxStateEntity(x => x.ToTable("outbox_state"));
    }
}
