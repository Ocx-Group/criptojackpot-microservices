using CryptoJackpot.Lottery.Domain.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CryptoJackpot.Lottery.Data.Context;

public class LotteryDbContext : DbContext
{
    public DbSet<LotteryDraw> LotteryDraws { get; set; }
    public DbSet<LotteryNumber> LotteryNumbers { get; set; }
    public DbSet<Prize> Prizes { get; set; }
    public DbSet<PrizeImage> PrizeImages { get; set; }

    public LotteryDbContext(DbContextOptions<LotteryDbContext> options) : base(options) {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LotteryDbContext).Assembly);

        // MassTransit Outbox configuration with snake_case naming
        modelBuilder.AddInboxStateEntity(x => x.ToTable("inbox_state"));
        modelBuilder.AddOutboxMessageEntity(x => x.ToTable("outbox_message"));
        modelBuilder.AddOutboxStateEntity(x => x.ToTable("outbox_state"));
    }
}