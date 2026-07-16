using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CryptoJackpot.Wallet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReferralRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "referral_relationships",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    referrer_user_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    referred_user_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    referral_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_referral_relationships", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_referral_relationships_referred_user_guid",
                table: "referral_relationships",
                column: "referred_user_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_referral_relationships_referrer_user_guid",
                table: "referral_relationships",
                column: "referrer_user_guid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "referral_relationships");
        }
    }
}
