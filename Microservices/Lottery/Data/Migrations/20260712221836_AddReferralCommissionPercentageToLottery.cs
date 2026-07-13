using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoJackpot.Lottery.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReferralCommissionPercentageToLottery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "referral_commission_percentage",
                table: "lottery_draws",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 1.00m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "referral_commission_percentage",
                table: "lottery_draws");
        }
    }
}
