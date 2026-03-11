using CryptoJackpot.Identity.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoJackpot.Identity.Data.Context.Configurations;

public class WishListItemConfiguration : IEntityTypeConfiguration<WishListItem>
{
    public void Configure(EntityTypeBuilder<WishListItem> builder)
    {
        builder.ToTable("wish_list_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.WishListItemGuid).IsRequired();
        builder.HasIndex(x => x.WishListItemGuid).IsUnique();

        builder.Property(x => x.UserGuid).IsRequired();
        builder.Property(x => x.LotteryGuid).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // A user can only add a lottery to their wishlist once
        builder.HasIndex(x => new { x.UserId, x.LotteryGuid }).IsUnique();
        builder.HasIndex(x => x.UserGuid);

        builder.HasQueryFilter(e => !e.DeletedAt.HasValue);
    }
}
