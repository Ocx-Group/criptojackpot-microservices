using CryptoJackpot.Wallet.Domain.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CryptoJackpot.Wallet.Data.Context;

public class WalletDbContext : DbContext
{
    public WalletDbContext(DbContextOptions<WalletDbContext> options) : base(options)
    {
    }

    public DbSet<UserCryptoWallet> UserCryptoWallets { get; set; } = null!;
    public DbSet<WalletTransaction> WalletTransactions { get; set; } = null!;
    public DbSet<WalletBalance> WalletBalances { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WalletDbContext).Assembly);

        // MassTransit Outbox configuration with snake_case naming
        modelBuilder.AddInboxStateEntity(x => x.ToTable("inbox_state"));
        modelBuilder.AddOutboxMessageEntity(x => x.ToTable("outbox_message"));
        modelBuilder.AddOutboxStateEntity(x => x.ToTable("outbox_state"));
    }
}
