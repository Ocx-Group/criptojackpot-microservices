using CryptoJackpot.Wallet.Domain.Enums;
using CryptoJackpot.Wallet.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoJackpot.Wallet.Data.Configuration;

public class WalletConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> builder)
    {
        builder.ToTable("wallet_transactions");

        builder.HasIndex(t => t.TransactionGuid).IsUnique();
        builder.HasIndex(t => t.UserGuid);
        builder.HasIndex(t => t.ReferenceId);
        builder.HasIndex(t => t.Status);

        builder.Property(t => t.TransactionGuid).IsRequired();
        builder.Property(t => t.UserGuid).IsRequired();

        builder.Property(t => t.Amount)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(t => t.Direction)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(WalletTransactionStatus.Completed);

        builder.Property(t => t.BalanceAfter)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(t => t.Description)
            .HasMaxLength(500);
    }
}
