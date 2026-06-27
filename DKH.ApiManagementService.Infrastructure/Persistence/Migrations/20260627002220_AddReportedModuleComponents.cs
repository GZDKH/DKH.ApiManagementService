using System;
using System.Collections.Generic;
using DKH.ApiManagementService.Domain.Entities;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DKH.ApiManagementService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReportedModuleComponents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reported_module_components",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<Dictionary<string, string>>(type: "jsonb", nullable: false),
                    Version = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Description = table.Column<Dictionary<string, string>>(type: "jsonb", nullable: true),
                    Category = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Provides = table.Column<List<ReportedCapability>>(type: "jsonb", nullable: false),
                    Requires = table.Column<List<ReportedDependency>>(type: "jsonb", nullable: false),
                    RequiresEntitlement = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    State = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
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
                    table.PrimaryKey("PK_reported_module_components", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_reported_module_components_ModuleId",
                table: "reported_module_components",
                column: "ModuleId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reported_module_components");
        }
    }
}
