using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoJackpot.Order.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceIdToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "invoice_id",
                table: "orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_orders_invoice_id",
                table: "orders",
                column: "invoice_id",
                unique: true,
                filter: "invoice_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_orders_invoice_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "invoice_id",
                table: "orders");
        }
    }
}
