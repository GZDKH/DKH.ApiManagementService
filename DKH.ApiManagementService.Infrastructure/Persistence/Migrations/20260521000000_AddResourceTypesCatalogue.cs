using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DKH.ApiManagementService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddResourceTypesCatalogue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ADR-025 §5: catalogue table for resource types alongside grants.
            // Matches DKH.Platform.Authorization.ResourceAccess.EntityFrameworkCore.Configurations.ResourceTypeConfiguration schema.
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS resource_types (
                    ""Id"" varchar(64) NOT NULL,
                    ""DisplayName"" varchar(128) NOT NULL,
                    ""IsScopeOnly"" boolean NOT NULL,
                    ""ParentScopeTypes"" varchar(64)[] NULL,
                    ""GrantCreatorFullAccess"" boolean NOT NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    CONSTRAINT ""PK_resource_types"" PRIMARY KEY (""Id"")
                );
            ");

            // ApiManagementService owns two resource types: api_key (per-customer)
            // and ai_provider (AdminGateway-managed). Neither has external scope.
            migrationBuilder.Sql(@"
                INSERT INTO resource_types
                    (""Id"", ""DisplayName"", ""IsScopeOnly"", ""ParentScopeTypes"", ""GrantCreatorFullAccess"", ""CreatedAt"")
                VALUES
                    ('api_key', 'API Key', false, NULL, true, NOW()),
                    ('ai_provider', 'AI Provider', false, NULL, true, NOW())
                ON CONFLICT (""Id"") DO NOTHING;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS resource_types;");
        }
    }
}
