using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CryptoJackpot.Winner.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLotteryWinners : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "lottery_winners",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    winner_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    lottery_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lottery_title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    number = table.Column<int>(type: "integer", nullable: false),
                    series = table.Column<int>(type: "integer", nullable: false),
                    ticket_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    purchase_amount = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    user_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    prize_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    prize_estimated_value = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    prize_image_url = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    won_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lottery_winners", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_lottery_winners_lottery_id",
                table: "lottery_winners",
                column: "lottery_id");

            migrationBuilder.CreateIndex(
                name: "ix_lottery_winners_lottery_number_series",
                table: "lottery_winners",
                columns: new[] { "lottery_id", "number", "series" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lottery_winners_user_id",
                table: "lottery_winners",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_lottery_winners_winner_guid",
                table: "lottery_winners",
                column: "winner_guid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lottery_winners");
        }
    }
}
