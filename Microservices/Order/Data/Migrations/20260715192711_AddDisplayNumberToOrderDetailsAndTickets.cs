using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoJackpot.Order.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDisplayNumberToOrderDetailsAndTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "display_number",
                table: "tickets",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "display_number",
                table: "order_details",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            // Backfill legacy rows. The Order DB doesn't know each lottery's number
            // range, so it uses the platform rules: Pick3 (lottery_type = 5) is
            // 3 digits and the standard game is 0-9999 (4 digits).
            migrationBuilder.Sql("""
                UPDATE order_details od
                SET display_number = lpad(od.number::text,
                    CASE WHEN o.lottery_type = 5 THEN 3 ELSE 4 END, '0')
                FROM orders o
                WHERE o.id = od.order_id;

                UPDATE tickets t
                SET display_number = lpad(t.number::text,
                    CASE WHEN o.lottery_type = 5 THEN 3 ELSE 4 END, '0')
                FROM order_details od
                JOIN orders o ON o.id = od.order_id
                WHERE od.id = t.order_detail_id;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "display_number",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "display_number",
                table: "order_details");
        }
    }
}
