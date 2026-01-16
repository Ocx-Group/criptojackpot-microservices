using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoJackpot.Order.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderAndTicketTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tickets",
                columns: table => new
                {
                    ticket_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lottery_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    purchase_amount = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    purchase_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    transaction_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    selected_numbers = table.Column<int[]>(type: "integer[]", nullable: false),
                    series = table.Column<int>(type: "integer", nullable: false),
                    lottery_number_ids = table.Column<List<Guid>>(type: "uuid[]", nullable: false),
                    is_gift = table.Column<bool>(type: "boolean", nullable: false),
                    gift_recipient_id = table.Column<long>(type: "bigint", nullable: true),
                    won_prize_ids = table.Column<List<long>>(type: "bigint[]", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tickets", x => x.ticket_guid);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    order_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    lottery_id = table.Column<Guid>(type: "uuid", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    selected_numbers = table.Column<int[]>(type: "integer[]", nullable: false),
                    series = table.Column<int>(type: "integer", nullable: false),
                    lottery_number_ids = table.Column<List<Guid>>(type: "uuid[]", nullable: false),
                    is_gift = table.Column<bool>(type: "boolean", nullable: false),
                    gift_recipient_id = table.Column<long>(type: "bigint", nullable: true),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orders", x => x.order_guid);
                    table.ForeignKey(
                        name: "fk_orders_tickets_ticket_id",
                        column: x => x.ticket_id,
                        principalTable: "tickets",
                        principalColumn: "ticket_guid");
                });

            migrationBuilder.CreateIndex(
                name: "ix_orders_lottery_id",
                table: "orders",
                column: "lottery_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_status_expires_at",
                table: "orders",
                columns: new[] { "status", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_orders_ticket_id",
                table: "orders",
                column: "ticket_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_orders_user_id",
                table: "orders",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_lottery_id",
                table: "tickets",
                column: "lottery_id");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_order_id",
                table: "tickets",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_user_id",
                table: "tickets",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "tickets");
        }
    }
}
