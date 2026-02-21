# Domain model

## Entities

### ApiKeyEntity (aggregate root)

Inherits from `FullAuditedEntityWithKey<Guid>` (provides `Id`, `CreationTime`, `CreatorId`, `LastModificationTime`, `LastModifierId`, `IsDeleted`, `DeleterId`, `DeletionTime`).

| Property | Type | Description |
|----------|------|-------------|
| Id | `Guid` | Primary key (from base class) |
| Name | `string` | Human-readable key name |
| KeyHash | `string` | SHA-256 hash of the raw key |
| KeyPrefix | `string` | First characters of raw key (for identification) |
| Scope | `ApiKeyScope` | Access scope |
| Status | `ApiKeyStatus` | Current status |
| Permissions | `List<string>` | Granted permissions |
| Description | `string?` | Optional description |
| ExpiresAt | `DateTimeOffset?` | Optional expiration |
| LastUsedAt | `DateTimeOffset?` | Last usage timestamp |
| UsageRecords | `IReadOnlyCollection<ApiKeyUsageEntity>` | Navigation to usage records |
| CreationTime | `DateTime` | Creation timestamp (from base class) |
| CreatorId | `Guid?` | Creator user ID (from base class) |
| LastModificationTime | `DateTime?` | Last modification timestamp (from base class) |
| LastModifierId | `Guid?` | Last modifier user ID (from base class) |
| IsDeleted | `bool` | Soft-delete flag (from base class) |
| DeleterId | `Guid?` | Deleter user ID (from base class) |
| DeletionTime | `DateTime?` | Deletion timestamp (from base class) |

**Behavior methods:**
- `Create(name, keyHash, keyPrefix, scope, permissions, description?, expiresAt?)` — factory method, raises `ApiKeyCreatedDomainEvent`
- `Update(name?, description?, permissions?, expiresAt?)` — update mutable fields
- `Revoke()` — set status to Revoked, raises `ApiKeyRevokedDomainEvent`
- `Regenerate(newKeyHash, newKeyPrefix)` — replace key hash/prefix, raises `ApiKeyRegeneratedDomainEvent`
- `RecordUsage()` — update `LastUsedAt` to current UTC time
- `IsExpired()` — checks if expiration date has passed
- `IsActive()` — checks status is Active and not expired
- `HasPermission(permission)` — checks permission list (case-insensitive)

### ApiKeyUsageEntity

Inherits from `Entity<Guid>` (provides `Id`).

| Property | Type | Description |
|----------|------|-------------|
| Id | `Guid` | Primary key (from base class) |
| ApiKeyId | `Guid` | Foreign key to ApiKey |
| Endpoint | `string` | Called endpoint |
| StatusCode | `int` | Response status code |
| IpAddress | `string?` | Client IP address |
| UserAgent | `string?` | Client user agent string |
| Timestamp | `DateTimeOffset` | Usage timestamp |
| ResponseTimeMs | `long` | Response time in milliseconds |
| ApiKey | `ApiKeyEntity` | Navigation property to parent key |

**Factory method:**
- `Create(apiKeyId, endpoint, statusCode, ipAddress?, userAgent?, responseTimeMs)` — creates usage record with current UTC timestamp

## Value objects

### ApiKeyHash

Encapsulates SHA-256 hashing of raw API keys.

- `FromRawKey(rawKey)` — computes SHA-256 hash from raw key string
- `FromHash(hash)` — wraps an existing hex-encoded hash
- `Value` — the hex-encoded hash string (lowercase)

## Enums

### ApiKeyScope

| Value | Code | Description |
|-------|------|-------------|
| Mcp | `mcp` | MCP gateway access |
| Webhook | `wh` | Webhook integrations |
| Partner | `ptr` | Partner API access |
| Storefront | `sf` | Storefront API access |
| Internal | `int` | Internal service communication |

### ApiKeyStatus

| Value | Description |
|-------|-------------|
| Active | Key is active and usable |
| Revoked | Key has been manually revoked |
| Expired | Key has passed its expiration date |

## Domain events

### ApiKeyCreatedDomainEvent

Raised when a new API key is created.

| Property | Type | Description |
|----------|------|-------------|
| ApiKeyId | `Guid` | ID of the created key |
| Name | `string` | Key name |
| Scope | `ApiKeyScope` | Key scope |
| OccurredOnUtc | `DateTime` | Event timestamp |

### ApiKeyRevokedDomainEvent

Raised when an API key is revoked.

| Property | Type | Description |
|----------|------|-------------|
| ApiKeyId | `Guid` | ID of the revoked key |
| Name | `string` | Key name |
| OccurredOnUtc | `DateTime` | Event timestamp |

### ApiKeyRegeneratedDomainEvent

Raised when an API key is regenerated.

| Property | Type | Description |
|----------|------|-------------|
| ApiKeyId | `Guid` | ID of the regenerated key |
| Name | `string` | Key name |
| OccurredOnUtc | `DateTime` | Event timestamp |

## API key format

Pattern: `dkh_{scope}_{random32}`

Examples:
- `dkh_mcp_a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6`
- `dkh_wh_x9y8z7w6v5u4t3s2r1q0p9o8n7m6l5k4`

The raw key is returned once at creation time. Only the SHA-256 hash is stored.

*Last updated: February 2026*
