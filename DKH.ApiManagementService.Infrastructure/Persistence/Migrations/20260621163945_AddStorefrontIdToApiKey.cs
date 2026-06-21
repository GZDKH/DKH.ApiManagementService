using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DKH.ApiManagementService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStorefrontIdToApiKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StorefrontId",
                table: "api_keys",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_api_keys_CustomerId_StorefrontId",
                table: "api_keys",
                columns: new[] { "CustomerId", "StorefrontId" });

            migrationBuilder.CreateIndex(
                name: "IX_api_keys_StorefrontId",
                table: "api_keys",
                column: "StorefrontId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_api_keys_CustomerId_StorefrontId",
                table: "api_keys");

            migrationBuilder.DropIndex(
                name: "IX_api_keys_StorefrontId",
                table: "api_keys");

            migrationBuilder.DropColumn(
                name: "StorefrontId",
                table: "api_keys");
        }
    }
}
