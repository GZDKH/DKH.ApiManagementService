using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DKH.ApiManagementService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWebhookSubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "webhook_subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApiKeyId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CallbackUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Events = table.Column<string>(type: "jsonb", nullable: false),
                    SigningSecretHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SigningSecretPrefix = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RetryMaxAttempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 5),
                    RetryBackoffSeconds = table.Column<int>(type: "integer", nullable: false, defaultValue: 30),
                    DlqEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    LastDeliveryAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastDeliverySucceeded = table.Column<bool>(type: "boolean", nullable: true),
                    LastDeliveryStatusCode = table.Column<int>(type: "integer", nullable: true),
                    LastDeliveryError = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    FailureCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastRotatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RotationCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_webhook_subscriptions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_webhook_subscriptions_ApiKeyId",
                table: "webhook_subscriptions",
                column: "ApiKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_webhook_subscriptions_CustomerId",
                table: "webhook_subscriptions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_webhook_subscriptions_CustomerId_Status",
                table: "webhook_subscriptions",
                columns: new[] { "CustomerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_webhook_subscriptions_Status",
                table: "webhook_subscriptions",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "webhook_subscriptions");
        }
    }
}
