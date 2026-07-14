using System.Collections.Generic;
using CryptoJackpot.Lottery.Domain.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoJackpot.Lottery.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTranslationsToLotteryAndPrize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Dictionary<string, PrizeTranslation>>(
                name: "translations",
                table: "prizes",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<Dictionary<string, LotteryTranslation>>(
                name: "translations",
                table: "lottery_draws",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "translations",
                table: "prizes");

            migrationBuilder.DropColumn(
                name: "translations",
                table: "lottery_draws");
        }
    }
}
