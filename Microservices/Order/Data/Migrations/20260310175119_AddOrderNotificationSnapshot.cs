using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoJackpot.Order.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderNotificationSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "lottery_no",
                table: "orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "lottery_title",
                table: "orders",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "user_email",
                table: "orders",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "user_name",
                table: "orders",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "lottery_no",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "lottery_title",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "user_email",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "user_name",
                table: "orders");
        }
    }
}
