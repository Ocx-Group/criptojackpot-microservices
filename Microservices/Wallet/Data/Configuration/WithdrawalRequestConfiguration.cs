using CryptoJackpot.Wallet.Domain.Enums;
using CryptoJackpot.Wallet.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoJackpot.Wallet.Data.Configuration;

public class WithdrawalRequestConfiguration : IEntityTypeConfiguration<WithdrawalRequest>
{
    public void Configure(EntityTypeBuilder<WithdrawalRequest> builder)
    {
        builder.ToTable("withdrawal_requests");

        builder.HasIndex(r => r.RequestGuid).IsUnique();
        builder.HasIndex(r => r.UserGuid);
        builder.HasIndex(r => r.Status);

        builder.Property(r => r.RequestGuid).IsRequired();
        builder.Property(r => r.UserGuid).IsRequired();

        builder.Property(r => r.Amount)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(WithdrawalRequestStatus.Pending);

        builder.Property(r => r.WalletAddress)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(r => r.CurrencySymbol)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(r => r.CurrencyName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.AdminNotes)
            .HasMaxLength(500);
    }
}
