using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DKH.ApiManagementService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class _202603220001_AddAiProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ai_providers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ProviderType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    BaseUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Models = table.Column<string>(type: "jsonb", nullable: false),
                    ApiKeyReference = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    RateLimitPerMinute = table.Column<int>(type: "integer", nullable: true),
                    DailyQuota = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_ai_providers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ai_providers_Name",
                table: "ai_providers",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ai_providers_ProviderType",
                table: "ai_providers",
                column: "ProviderType");

            migrationBuilder.CreateIndex(
                name: "IX_ai_providers_Status",
                table: "ai_providers",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_providers");
        }
    }
}
