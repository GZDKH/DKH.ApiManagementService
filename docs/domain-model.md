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
| CustomerId | `Guid?` | Customer/tenant owner for partner-facing keys |
| Environment | `ApiKeyEnvironment` | Sandbox or production key environment |
| RateLimitTier | `ApiKeyRateLimitTier` | Configured rate-limit tier |
| RateLimitRequestsPerMinute | `int` | Resolved per-minute request limit for the environment/tier |
| Description | `string?` | Optional description |
| ExpiresAt | `DateTimeOffset?` | Optional expiration |
| LastUsedAt | `DateTimeOffset?` | Last usage timestamp |
| LastRotatedAt | `DateTimeOffset?` | Last key rotation timestamp |
| PreviousKeyPrefix | `string?` | Previous key prefix after rotation |
| RotationCount | `int` | Number of completed key rotations |
| UsageRecords | `IReadOnlyCollection<ApiKeyUsageEntity>` | Navigation to usage records |
| CreationTime | `DateTime` | Creation timestamp (from base class) |
| CreatorId | `Guid?` | Creator user ID (from base class) |
| LastModificationTime | `DateTime?` | Last modification timestamp (from base class) |
| LastModifierId | `Guid?` | Last modifier user ID (from base class) |
| IsDeleted | `bool` | Soft-delete flag (from base class) |
| DeleterId | `Guid?` | Deleter user ID (from base class) |
| DeletionTime | `DateTime?` | Deletion timestamp (from base class) |

**Behavior methods:**
- `Create(name, keyHash, keyPrefix, scope, permissions, customerId?, environment, rateLimitTier, description?, expiresAt?)` — factory method, raises `ApiKeyCreatedDomainEvent`
- `Update(name?, description?, permissions?, expiresAt?, customerId?, environment?, rateLimitTier?)` — update mutable fields and public API policy metadata
- `Revoke()` — set status to Revoked, raises `ApiKeyRevokedDomainEvent`
- `Regenerate(newKeyHash, newKeyPrefix)` — replace key hash/prefix, records rotation metadata, raises `ApiKeyRegeneratedDomainEvent`
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
| CustomerId | `Guid?` | Customer/tenant owner copied from the key at record time |
| Environment | `ApiKeyEnvironment` | Key environment copied at record time |
| RateLimitTier | `ApiKeyRateLimitTier` | Rate-limit tier copied at record time |
| RateLimitRequestsPerMinute | `int` | Resolved limit copied at record time |
| Timestamp | `DateTimeOffset` | Usage timestamp |
| ResponseTimeMs | `long` | Response time in milliseconds |
| ApiKey | `ApiKeyEntity` | Navigation property to parent key |

**Factory method:**
- `Create(apiKeyId, endpoint, statusCode, ipAddress?, userAgent?, responseTimeMs, customerId?, environment, rateLimitTier, rateLimitRequestsPerMinute)` — creates usage record with current UTC timestamp and analytics dimensions

### WebhookSubscriptionEntity (aggregate root)

Inherits from `FullAuditedEntityWithKey<Guid>`.

| Property | Type | Description |
|----------|------|-------------|
| Id | `Guid` | Primary key |
| ApiKeyId | `Guid?` | API key that owns the subscription |
| CustomerId | `Guid?` | Customer/tenant owner for partner-facing subscriptions |
| Name | `string` | Human-readable subscription name |
| CallbackUrl | `string` | Partner callback endpoint |
| Events | `List<string>` | Normalized subscribed event names |
| SigningSecretHash | `string` | SHA-256 hash of the webhook signing secret |
| SigningSecretPrefix | `string` | Display prefix only; raw secrets are never stored |
| Status | `WebhookSubscriptionStatus` | Active/disabled lifecycle state |
| RetryMaxAttempts | `int` | Max retry attempts before DLQ |
| RetryBackoffSeconds | `int` | Retry backoff interval in seconds |
| DlqEnabled | `bool` | Whether failed deliveries can move to DLQ |
| LastDeliveryAt | `DateTimeOffset?` | Last delivery attempt timestamp |
| LastDeliverySucceeded | `bool?` | Last delivery result |
| LastDeliveryStatusCode | `int?` | Last partner HTTP status |
| LastDeliveryError | `string?` | Last delivery error summary |
| FailureCount | `int` | Consecutive failed delivery count |
| LastRotatedAt | `DateTimeOffset?` | Last signing-secret rotation timestamp |
| RotationCount | `int` | Number of completed signing-secret rotations |

**Behavior methods:**
- `Create(apiKeyId?, customerId?, name, callbackUrl, events, rawSigningSecret, retryMaxAttempts, retryBackoffSeconds, dlqEnabled)` — normalizes events and stores only the signing secret hash/prefix
- `Update(name, callbackUrl, events, retryMaxAttempts, retryBackoffSeconds, dlqEnabled)` — updates route and retry/DLQ policy
- `Disable()` — disables delivery without deleting the subscription
- `RotateSecret(rawSigningSecret)` — replaces hash/prefix and records rotation telemetry
- `RecordDelivery(succeeded, statusCode, error, deliveredAt)` — records subscriber observability fields

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

### ApiKeyEnvironment

| Value | Description |
|-------|-------------|
| Sandbox | Non-production partner testing |
| Production | Live partner traffic |

### ApiKeyRateLimitTier

| Value | Requests/minute |
|-------|-----------------|
| Development | 60 |
| Standard | 600 |
| Professional | 3,000 |
| Enterprise | 12,000 |

Sandbox keys are capped at 120 requests/minute regardless of tier.

### WebhookSubscriptionStatus

| Value | Description |
|-------|-------------|
| Active | Subscription can receive deliveries |
| Disabled | Subscription remains stored but delivery is disabled |

## Domain services

### WebhookSigningSecretHasher

Computes a lowercase SHA-256 hash of a raw signing secret and derives a display-only prefix. Raw webhook signing secrets are never stored.

### WebhookSignatureService

Produces deterministic HMAC-SHA256 signatures over `timestamp.payload` with the `sha256=` prefix for outbound webhook delivery.

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

*Last updated: June 2026*
