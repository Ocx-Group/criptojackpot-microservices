using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoJackpot.Lottery.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderIdAndTicketIdToLotteryNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LotteryNumbers_LotteryId_Number_Series",
                table: "lottery_numbers");

            migrationBuilder.DropColumn(
                name: "is_available",
                table: "lottery_numbers");

            migrationBuilder.AddColumn<Guid>(
                name: "order_id",
                table: "lottery_numbers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "reservation_expires_at",
                table: "lottery_numbers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "lottery_numbers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_LotteryNumbers_LotteryId_Number_Series",
                table: "lottery_numbers",
                columns: new[] { "lottery_id", "number", "series" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LotteryNumbers_LotteryId_Status",
                table: "lottery_numbers",
                columns: new[] { "lottery_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_LotteryNumbers_OrderId",
                table: "lottery_numbers",
                column: "order_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LotteryNumbers_LotteryId_Number_Series",
                table: "lottery_numbers");

            migrationBuilder.DropIndex(
                name: "IX_LotteryNumbers_LotteryId_Status",
                table: "lottery_numbers");

            migrationBuilder.DropIndex(
                name: "IX_LotteryNumbers_OrderId",
                table: "lottery_numbers");

            migrationBuilder.DropColumn(
                name: "order_id",
                table: "lottery_numbers");

            migrationBuilder.DropColumn(
                name: "reservation_expires_at",
                table: "lottery_numbers");

            migrationBuilder.DropColumn(
                name: "status",
                table: "lottery_numbers");

            migrationBuilder.AddColumn<bool>(
                name: "is_available",
                table: "lottery_numbers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_LotteryNumbers_LotteryId_Number_Series",
                table: "lottery_numbers",
                columns: new[] { "lottery_id", "number", "series" });
        }
    }
}
