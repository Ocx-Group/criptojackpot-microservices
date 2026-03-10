using CryptoJackpot.Wallet.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoJackpot.Wallet.Data.Configuration;

public class ReferralRelationshipConfiguration : IEntityTypeConfiguration<ReferralRelationship>
{
    public void Configure(EntityTypeBuilder<ReferralRelationship> builder)
    {
        builder.ToTable("referral_relationships");

        builder.Property(r => r.ReferrerUserGuid).IsRequired();
        builder.Property(r => r.ReferredUserGuid).IsRequired();

        builder.Property(r => r.ReferralCode)
            .IsRequired()
            .HasMaxLength(50);

        // Each referred user can only have one referrer
        builder.HasIndex(r => r.ReferredUserGuid)
            .IsUnique();

        builder.HasIndex(r => r.ReferrerUserGuid);
    }
}

