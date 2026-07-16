using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoJackpot.Lottery.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDisplayNumberToLotteryNumbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "display_number",
                table: "lottery_numbers",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            // Backfill existing numbers with the same rule used at generation time:
            // Pick3 (type = 5) is always 3 digits; other types pad to the width of
            // the lottery's max_number (min 2), e.g. max_number 9999 -> "0007".
            migrationBuilder.Sql("""
                UPDATE lottery_numbers ln
                SET display_number = lpad(ln.number::text,
                    CASE WHEN ld.type = 5 THEN 3
                         ELSE GREATEST(2, length(ld.max_number::text))
                    END, '0')
                FROM lottery_draws ld
                WHERE ld.id = ln.lottery_id;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "display_number",
                table: "lottery_numbers");
        }
    }
}
