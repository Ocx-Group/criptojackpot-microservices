using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoJackpot.Order.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLotteryTypeToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "lottery_type",
                table: "orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "lottery_type",
                table: "orders");
        }
    }
}
