using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CryptoJackpot.Wallet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCryptoWallets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_crypto_wallets",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    wallet_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    address = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    currency_symbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    currency_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    logo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_crypto_wallets", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_crypto_wallets_user_guid",
                table: "user_crypto_wallets",
                column: "user_guid");

            migrationBuilder.CreateIndex(
                name: "ix_user_crypto_wallets_wallet_guid",
                table: "user_crypto_wallets",
                column: "wallet_guid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_crypto_wallets");
        }
    }
}
