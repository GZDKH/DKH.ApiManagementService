# Domain model

## Entities

### ApiKeyEntity (aggregate root)

| Property | Type | Description |
|----------|------|-------------|
| Id | `Guid` | Primary key |
| Name | `string` | Human-readable key name |
| Description | `string?` | Optional description |
| KeyHash | `string` | SHA-256 hash of the raw key |
| KeyPrefix | `string` | First 12 characters of raw key (for identification) |
| Scope | `ApiKeyScope` | Access scope |
| Status | `ApiKeyStatus` | Current status |
| Permissions | `List<string>` | Granted permissions |
| ExpiresAt | `DateTimeOffset?` | Optional expiration |
| LastUsedAt | `DateTimeOffset?` | Last usage timestamp |
| TotalRequests | `long` | Total request count |
| CreatedAt | `DateTimeOffset` | Creation timestamp |
| UpdatedAt | `DateTimeOffset?` | Last update timestamp |

**Behavior methods:**
- `Update(name, description, permissions, expiresAt)` — update mutable fields
- `Revoke()` — set status to Revoked
- `Regenerate(keyHash, keyPrefix)` — replace key hash/prefix, reset usage counters
- `RecordUsage()` — increment counter, update `LastUsedAt`
- `IsActive` — checks status and expiration
- `HasPermission(permission)` — checks permission list

### ApiKeyUsageEntity

| Property | Type | Description |
|----------|------|-------------|
| Id | `Guid` | Primary key |
| ApiKeyId | `Guid` | Foreign key to ApiKey |
| Endpoint | `string` | Called endpoint |
| IpAddress | `string?` | Client IP address |
| StatusCode | `int` | Response status code |
| Timestamp | `DateTimeOffset` | Usage timestamp |

## Value objects

### ApiKeyHash

Encapsulates SHA-256 hashing of raw API keys.

- `Create(rawKey)` — computes SHA-256 hash
- `Value` — the hex-encoded hash string

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

## API key format

Pattern: `dkh_{scope}_{random32}`

Examples:
- `dkh_mcp_a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6`
- `dkh_wh_x9y8z7w6v5u4t3s2r1q0p9o8n7m6l5k4`

The raw key is returned once at creation time. Only the SHA-256 hash is stored.
