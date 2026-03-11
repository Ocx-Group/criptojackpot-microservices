using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CryptoJackpot.Identity.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWishListItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "wish_list_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    wish_list_item_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    user_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    lottery_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wish_list_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_wish_list_items_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_wish_list_items_user_guid",
                table: "wish_list_items",
                column: "user_guid");

            migrationBuilder.CreateIndex(
                name: "ix_wish_list_items_user_id_lottery_guid",
                table: "wish_list_items",
                columns: new[] { "user_id", "lottery_guid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_wish_list_items_wish_list_item_guid",
                table: "wish_list_items",
                column: "wish_list_item_guid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "wish_list_items");
        }
    }
}
