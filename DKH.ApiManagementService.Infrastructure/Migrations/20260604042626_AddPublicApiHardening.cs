using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DKH.ApiManagementService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicApiHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "api_keys",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Environment",
                table: "api_keys",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Production");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastRotatedAt",
                table: "api_keys",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousKeyPrefix",
                table: "api_keys",
                type: "character varying(48)",
                maxLength: 48,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RateLimitRequestsPerMinute",
                table: "api_keys",
                type: "integer",
                nullable: false,
                defaultValue: 600);

            migrationBuilder.AddColumn<string>(
                name: "RateLimitTier",
                table: "api_keys",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Standard");

            migrationBuilder.AddColumn<int>(
                name: "RotationCount",
                table: "api_keys",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "api_key_usage",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Environment",
                table: "api_key_usage",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Production");

            migrationBuilder.AddColumn<int>(
                name: "RateLimitRequestsPerMinute",
                table: "api_key_usage",
                type: "integer",
                nullable: false,
                defaultValue: 600);

            migrationBuilder.AddColumn<string>(
                name: "RateLimitTier",
                table: "api_key_usage",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Standard");

            migrationBuilder.CreateIndex(
                name: "IX_api_keys_CustomerId",
                table: "api_keys",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_api_keys_CustomerId_Environment",
                table: "api_keys",
                columns: new[] { "CustomerId", "Environment" });

            migrationBuilder.CreateIndex(
                name: "IX_api_keys_Environment",
                table: "api_keys",
                column: "Environment");

            migrationBuilder.CreateIndex(
                name: "IX_api_keys_RateLimitTier",
                table: "api_keys",
                column: "RateLimitTier");

            migrationBuilder.CreateIndex(
                name: "IX_api_key_usage_CustomerId",
                table: "api_key_usage",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_api_key_usage_CustomerId_Environment",
                table: "api_key_usage",
                columns: new[] { "CustomerId", "Environment" });

            migrationBuilder.CreateIndex(
                name: "IX_api_key_usage_Environment",
                table: "api_key_usage",
                column: "Environment");

            migrationBuilder.CreateIndex(
                name: "IX_api_key_usage_RateLimitTier",
                table: "api_key_usage",
                column: "RateLimitTier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_api_keys_CustomerId",
                table: "api_keys");

            migrationBuilder.DropIndex(
                name: "IX_api_keys_CustomerId_Environment",
                table: "api_keys");

            migrationBuilder.DropIndex(
                name: "IX_api_keys_Environment",
                table: "api_keys");

            migrationBuilder.DropIndex(
                name: "IX_api_keys_RateLimitTier",
                table: "api_keys");

            migrationBuilder.DropIndex(
                name: "IX_api_key_usage_CustomerId",
                table: "api_key_usage");

            migrationBuilder.DropIndex(
                name: "IX_api_key_usage_CustomerId_Environment",
                table: "api_key_usage");

            migrationBuilder.DropIndex(
                name: "IX_api_key_usage_Environment",
                table: "api_key_usage");

            migrationBuilder.DropIndex(
                name: "IX_api_key_usage_RateLimitTier",
                table: "api_key_usage");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "api_keys");

            migrationBuilder.DropColumn(
                name: "Environment",
                table: "api_keys");

            migrationBuilder.DropColumn(
                name: "LastRotatedAt",
                table: "api_keys");

            migrationBuilder.DropColumn(
                name: "PreviousKeyPrefix",
                table: "api_keys");

            migrationBuilder.DropColumn(
                name: "RateLimitRequestsPerMinute",
                table: "api_keys");

            migrationBuilder.DropColumn(
                name: "RateLimitTier",
                table: "api_keys");

            migrationBuilder.DropColumn(
                name: "RotationCount",
                table: "api_keys");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "api_key_usage");

            migrationBuilder.DropColumn(
                name: "Environment",
                table: "api_key_usage");

            migrationBuilder.DropColumn(
                name: "RateLimitRequestsPerMinute",
                table: "api_key_usage");

            migrationBuilder.DropColumn(
                name: "RateLimitTier",
                table: "api_key_usage");
        }
    }
}
