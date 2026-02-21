# Database

## Connection

- Database: `dkh_api_management`
- Provider: PostgreSQL 18 (via DKH.Platform.EntityFrameworkCore.PostgreSQL)
- EF Core version: 10.0.2
- Connection string key: `ConnectionStrings:Default`

## Schema

### api_keys

| Column | Type | Constraints |
|--------|------|-------------|
| Id | `uuid` | PK |
| Name | `character varying(256)` | NOT NULL |
| KeyHash | `character varying(64)` | NOT NULL, UNIQUE |
| KeyPrefix | `character varying(48)` | NOT NULL |
| Scope | `character varying(32)` | NOT NULL (enum as string) |
| Status | `character varying(32)` | NOT NULL (enum as string) |
| Permissions | `jsonb` | NOT NULL |
| Description | `character varying(1024)` | NULLABLE |
| ExpiresAt | `timestamptz` | NULLABLE |
| LastUsedAt | `timestamptz` | NULLABLE |
| CreationTime | `timestamptz` | NOT NULL (from FullAuditedEntity) |
| CreatorId | `uuid` | NULLABLE (from FullAuditedEntity) |
| LastModificationTime | `timestamptz` | NULLABLE (from FullAuditedEntity) |
| LastModifierId | `uuid` | NULLABLE (from FullAuditedEntity) |
| IsDeleted | `boolean` | NOT NULL (from FullAuditedEntity) |
| DeleterId | `uuid` | NULLABLE (from FullAuditedEntity) |
| DeletionTime | `timestamptz` | NULLABLE (from FullAuditedEntity) |

**Indexes:**
- `IX_api_keys_KeyHash` — unique index on `KeyHash`
- `IX_api_keys_Scope` — index on `Scope`
- `IX_api_keys_Status` — index on `Status`

### api_key_usage

| Column | Type | Constraints |
|--------|------|-------------|
| Id | `uuid` | PK |
| ApiKeyId | `uuid` | FK -> api_keys, NOT NULL |
| Endpoint | `character varying(512)` | NOT NULL |
| StatusCode | `integer` | NOT NULL |
| IpAddress | `character varying(45)` | NULLABLE |
| UserAgent | `character varying(512)` | NULLABLE |
| Timestamp | `timestamptz` | NOT NULL |
| ResponseTimeMs | `bigint` | NOT NULL |

**Indexes:**
- `IX_api_key_usage_ApiKeyId` — FK index
- `IX_api_key_usage_Timestamp` — index on `Timestamp`

**Foreign keys:**
- `FK_api_key_usage_api_keys_ApiKeyId` -> `api_keys(Id)` ON DELETE CASCADE

## Migrations

| Migration | Date | Description |
|-----------|------|-------------|
| `202502100001_InitialCreate` | 2026-02-10 | Create `api_keys` and `api_key_usage` tables |
| `RemoveDuplicateCreatedBy` | 2026-02-15 | Drop duplicate `CreatedBy` column from `api_keys` |

```bash
# Add migration
dotnet ef migrations add <Name> \
  --startup-project DKH.ApiManagementService.Api \
  --project DKH.ApiManagementService.Infrastructure

# Apply migrations
dotnet ef database update \
  --startup-project DKH.ApiManagementService.Api \
  --project DKH.ApiManagementService.Infrastructure
```

*Last updated: February 2026*
