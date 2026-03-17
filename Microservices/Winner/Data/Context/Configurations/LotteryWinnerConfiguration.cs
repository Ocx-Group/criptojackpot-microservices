using CryptoJackpot.Winner.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoJackpot.Winner.Data.Context.Configurations;

public class LotteryWinnerConfiguration : IEntityTypeConfiguration<LotteryWinner>
{
    public void Configure(EntityTypeBuilder<LotteryWinner> builder)
    {
        builder.ToTable("lottery_winners");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(w => w.WinnerGuid)
            .HasColumnName("winner_guid")
            .IsRequired();

        builder.HasIndex(w => w.WinnerGuid)
            .IsUnique()
            .HasDatabaseName("ix_lottery_winners_winner_guid");

        builder.Property(w => w.LotteryId)
            .HasColumnName("lottery_id")
            .IsRequired();

        builder.Property(w => w.LotteryTitle)
            .HasColumnName("lottery_title")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(w => w.Number)
            .HasColumnName("number")
            .IsRequired();

        builder.Property(w => w.Series)
            .HasColumnName("series")
            .IsRequired();

        builder.Property(w => w.TicketGuid)
            .HasColumnName("ticket_guid")
            .IsRequired();

        builder.Property(w => w.PurchaseAmount)
            .HasColumnName("purchase_amount")
            .HasColumnType("decimal(18,8)")
            .IsRequired();

        builder.Property(w => w.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(w => w.UserName)
            .HasColumnName("user_name")
            .HasMaxLength(256);

        builder.Property(w => w.UserEmail)
            .HasColumnName("user_email")
            .HasMaxLength(256);

        builder.Property(w => w.PrizeName)
            .HasColumnName("prize_name")
            .HasMaxLength(256);

        builder.Property(w => w.PrizeEstimatedValue)
            .HasColumnName("prize_estimated_value")
            .HasColumnType("decimal(18,2)");

        builder.Property(w => w.PrizeImageUrl)
            .HasColumnName("prize_image_url")
            .HasMaxLength(1024);

        builder.Property(w => w.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(w => w.WonAt)
            .HasColumnName("won_at")
            .IsRequired();

        builder.Property(w => w.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(w => w.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(w => w.DeletedAt)
            .HasColumnName("deleted_at");

        // Indexes
        builder.HasIndex(w => w.LotteryId).HasDatabaseName("ix_lottery_winners_lottery_id");
        builder.HasIndex(w => w.UserId).HasDatabaseName("ix_lottery_winners_user_id");
        builder.HasIndex(w => new { w.LotteryId, w.Number, w.Series })
            .IsUnique()
            .HasDatabaseName("ix_lottery_winners_lottery_number_series");
    }
}
