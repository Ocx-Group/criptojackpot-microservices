using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CryptoJackpot.Wallet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWithdrawalRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "withdrawal_requests",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    request_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    wallet_address = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    currency_symbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    currency_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    admin_notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    transaction_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_withdrawal_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_withdrawal_requests_wallet_transactions_transaction_id",
                        column: x => x.transaction_id,
                        principalTable: "wallet_transactions",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_withdrawal_requests_request_guid",
                table: "withdrawal_requests",
                column: "request_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_withdrawal_requests_status",
                table: "withdrawal_requests",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_withdrawal_requests_transaction_id",
                table: "withdrawal_requests",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_withdrawal_requests_user_guid",
                table: "withdrawal_requests",
                column: "user_guid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "withdrawal_requests");
        }
    }
}
