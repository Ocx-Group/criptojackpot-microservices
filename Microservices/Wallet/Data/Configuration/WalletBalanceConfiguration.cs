using CryptoJackpot.Wallet.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoJackpot.Wallet.Data.Configuration;

public class WalletBalanceConfiguration : IEntityTypeConfiguration<WalletBalance>
{
    public void Configure(EntityTypeBuilder<WalletBalance> builder)
    {
        builder.ToTable("wallet_balances");

        builder.HasIndex(b => b.UserGuid).IsUnique();

        builder.Property(b => b.UserGuid).IsRequired();

        builder.Property(b => b.Balance)
            .IsRequired()
            .HasPrecision(18, 4)
            .HasDefaultValue(0m);

        builder.Property(b => b.TotalEarned)
            .IsRequired()
            .HasPrecision(18, 4)
            .HasDefaultValue(0m);

        builder.Property(b => b.TotalWithdrawn)
            .IsRequired()
            .HasPrecision(18, 4)
            .HasDefaultValue(0m);

        builder.Property(b => b.TotalPurchased)
            .IsRequired()
            .HasPrecision(18, 4)
            .HasDefaultValue(0m);

        builder.Property(b => b.RowVersion)
            .IsRequired()
            .IsConcurrencyToken();

        builder.HasMany(b => b.Transactions)
            .WithOne(t => t.WalletBalance)
            .HasForeignKey(t => t.UserGuid)
            .HasPrincipalKey(b => b.UserGuid)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
