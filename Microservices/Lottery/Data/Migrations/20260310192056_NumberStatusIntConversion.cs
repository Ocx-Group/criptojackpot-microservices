using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoJackpot.Lottery.Data.Migrations
{
    /// <inheritdoc />
    public partial class NumberStatusIntConversion : Migration
    {
        // NumberStatus enum values:
        //   0 = Available
        //   1 = Reserved
        //   2 = Sold

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add a temporary integer column
            migrationBuilder.AddColumn<short>(
                name: "status_int",
                table: "lottery_numbers",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            // 2. Translate existing string values to their enum integer equivalents
            migrationBuilder.Sql("""
                UPDATE lottery_numbers SET status_int = CASE status
                    WHEN 'Available' THEN 0
                    WHEN 'Reserved'  THEN 1
                    WHEN 'Sold'      THEN 2
                    ELSE 0
                END;
                """);

            // 3. Drop the old string column and rename the new integer column
            migrationBuilder.DropColumn(name: "status", table: "lottery_numbers");
            migrationBuilder.RenameColumn(
                name: "status_int",
                table: "lottery_numbers",
                newName: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Add temporary string column
            migrationBuilder.AddColumn<string>(
                name: "status_str",
                table: "lottery_numbers",
                type: "text",
                nullable: false,
                defaultValue: "Available");

            // 2. Translate int values back to strings
            migrationBuilder.Sql("""
                UPDATE lottery_numbers SET status_str = CASE status
                    WHEN 0 THEN 'Available'
                    WHEN 1 THEN 'Reserved'
                    WHEN 2 THEN 'Sold'
                    ELSE 'Available'
                END;
                """);

            // 3. Drop int column and rename string column back
            migrationBuilder.DropColumn(name: "status", table: "lottery_numbers");
            migrationBuilder.RenameColumn(
                name: "status_str",
                table: "lottery_numbers",
                newName: "status");
        }
    }
}

