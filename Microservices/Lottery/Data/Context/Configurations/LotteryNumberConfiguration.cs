using CryptoJackpot.Lottery.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CryptoJackpot.Lottery.Data.Context.Configurations;

public class LotteryNumberConfiguration : IEntityTypeConfiguration<LotteryNumber>
{
    public void Configure(EntityTypeBuilder<LotteryNumber> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.LotteryId).IsRequired();
        builder.Property(e => e.Number).IsRequired();
        builder.Property(e => e.Series).IsRequired();
        builder.Property(e => e.IsAvailable).IsRequired();
        builder.Property(e => e.TicketId);
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
        
        builder.HasIndex(e => new { e.LotteryId, e.Number, e.Series })
            .HasDatabaseName("IX_LotteryNumbers_LotteryId_Number_Series").IsUnique();
        
        builder.HasQueryFilter(e => !e.DeletedAt.HasValue);
    }
}