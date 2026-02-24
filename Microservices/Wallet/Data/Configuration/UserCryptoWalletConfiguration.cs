using CryptoJackpot.Wallet.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoJackpot.Wallet.Data.Configuration;

public class UserCryptoWalletConfiguration : IEntityTypeConfiguration<UserCryptoWallet>
{
    public void Configure(EntityTypeBuilder<UserCryptoWallet> builder)
    {
        builder.ToTable("user_crypto_wallets");

        builder.HasIndex(w => w.WalletGuid).IsUnique();
        builder.HasIndex(w => w.UserGuid);

        builder.Property(w => w.WalletGuid).IsRequired();
        builder.Property(w => w.UserGuid).IsRequired();
        builder.Property(w => w.Address).IsRequired().HasMaxLength(256);
        builder.Property(w => w.CurrencySymbol).IsRequired().HasMaxLength(20);
        builder.Property(w => w.CurrencyName).IsRequired().HasMaxLength(100);
        builder.Property(w => w.LogoUrl).HasMaxLength(500);
        builder.Property(w => w.Label).IsRequired().HasMaxLength(100);
        builder.Property(w => w.IsDefault).HasDefaultValue(false);
    }
}
