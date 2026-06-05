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
| CustomerId | `uuid` | NULLABLE, customer/tenant owner for public API keys |
| Environment | `character varying(32)` | NOT NULL, default `Production` |
| RateLimitTier | `character varying(32)` | NOT NULL, default `Standard` |
| RateLimitRequestsPerMinute | `integer` | NOT NULL, default `600` |
| Description | `character varying(1024)` | NULLABLE |
| ExpiresAt | `timestamptz` | NULLABLE |
| LastUsedAt | `timestamptz` | NULLABLE |
| LastRotatedAt | `timestamptz` | NULLABLE |
| PreviousKeyPrefix | `character varying(48)` | NULLABLE |
| RotationCount | `integer` | NOT NULL, default `0` |
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
- `IX_api_keys_CustomerId` — index on `CustomerId`
- `IX_api_keys_Environment` — index on `Environment`
- `IX_api_keys_RateLimitTier` — index on `RateLimitTier`
- `IX_api_keys_CustomerId_Environment` — composite index for customer/environment key management

### api_key_usage

| Column | Type | Constraints |
|--------|------|-------------|
| Id | `uuid` | PK |
| ApiKeyId | `uuid` | FK -> api_keys, NOT NULL |
| Endpoint | `character varying(512)` | NOT NULL |
| StatusCode | `integer` | NOT NULL |
| IpAddress | `character varying(45)` | NULLABLE |
| UserAgent | `character varying(512)` | NULLABLE |
| CustomerId | `uuid` | NULLABLE, copied from the API key at record time |
| Environment | `character varying(32)` | NOT NULL, copied from the API key at record time |
| RateLimitTier | `character varying(32)` | NOT NULL, copied from the API key at record time |
| RateLimitRequestsPerMinute | `integer` | NOT NULL, copied from the API key at record time |
| Timestamp | `timestamptz` | NOT NULL |
| ResponseTimeMs | `bigint` | NOT NULL |

**Indexes:**
- `IX_api_key_usage_ApiKeyId` — FK index
- `IX_api_key_usage_Timestamp` — index on `Timestamp`
- `IX_api_key_usage_CustomerId` — index on `CustomerId`
- `IX_api_key_usage_Environment` — index on `Environment`
- `IX_api_key_usage_RateLimitTier` — index on `RateLimitTier`
- `IX_api_key_usage_CustomerId_Environment` — composite index for customer/environment analytics

**Foreign keys:**
- `FK_api_key_usage_api_keys_ApiKeyId` -> `api_keys(Id)` ON DELETE CASCADE

### webhook_subscriptions

| Column | Type | Constraints |
|--------|------|-------------|
| Id | `uuid` | PK |
| ApiKeyId | `uuid` | NULLABLE, API key that owns the subscription |
| CustomerId | `uuid` | NULLABLE, customer/tenant owner copied from the API key |
| Name | `character varying(256)` | NOT NULL |
| CallbackUrl | `character varying(2048)` | NOT NULL |
| Events | `jsonb` | NOT NULL, normalized event names |
| SigningSecretHash | `character varying(64)` | NOT NULL, SHA-256 hash of the signing secret |
| SigningSecretPrefix | `character varying(32)` | NOT NULL, display prefix only |
| Status | `character varying(32)` | NOT NULL (`Active`, `Disabled`) |
| RetryMaxAttempts | `integer` | NOT NULL, default `5` |
| RetryBackoffSeconds | `integer` | NOT NULL, default `30` |
| DlqEnabled | `boolean` | NOT NULL, default `true` |
| LastDeliveryAt | `timestamptz` | NULLABLE |
| LastDeliverySucceeded | `boolean` | NULLABLE |
| LastDeliveryStatusCode | `integer` | NULLABLE |
| LastDeliveryError | `character varying(2048)` | NULLABLE |
| FailureCount | `integer` | NOT NULL, default `0` |
| LastRotatedAt | `timestamptz` | NULLABLE |
| RotationCount | `integer` | NOT NULL, default `0` |
| CreationTime | `timestamptz` | NOT NULL (from FullAuditedEntity) |
| CreatorId | `uuid` | NULLABLE (from FullAuditedEntity) |
| LastModificationTime | `timestamptz` | NULLABLE (from FullAuditedEntity) |
| LastModifierId | `uuid` | NULLABLE (from FullAuditedEntity) |
| IsDeleted | `boolean` | NOT NULL (from FullAuditedEntity) |
| DeleterId | `uuid` | NULLABLE (from FullAuditedEntity) |
| DeletionTime | `timestamptz` | NULLABLE (from FullAuditedEntity) |

**Indexes:**
- `IX_webhook_subscriptions_ApiKeyId` — index on `ApiKeyId`
- `IX_webhook_subscriptions_CustomerId` — index on `CustomerId`
- `IX_webhook_subscriptions_Status` — index on `Status`
- `IX_webhook_subscriptions_CustomerId_Status` — composite index for customer-facing subscription lists

## Migrations

| Migration | Date | Description |
|-----------|------|-------------|
| `202502100001_InitialCreate` | 2026-02-10 | Create `api_keys` and `api_key_usage` tables |
| `RemoveDuplicateCreatedBy` | 2026-02-15 | Drop duplicate `CreatedBy` column from `api_keys` |
| `AddPublicApiHardening` | 2026-06-04 | Add customer ownership, sandbox/production environment, rate-limit tier, rotation metadata, and usage analytics dimensions |
| `AddWebhookSubscriptions` | 2026-06-05 | Add `webhook_subscriptions` table for partner callback URLs, event filters, HMAC secret metadata, retry/DLQ policy, and delivery observability |

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

*Last updated: June 2026*
