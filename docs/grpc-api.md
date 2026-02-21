# gRPC API

## Services

### ApiKeysCrudService

CRUD operations for API key management.

| Method | Request | Response | Description |
|--------|---------|----------|-------------|
| CreateApiKey | `CreateApiKeyRequest` | `CreateApiKeyResponse` | Create a new API key (returns raw key once) |
| GetApiKey | `GetApiKeyRequest` | `GetApiKeyResponse` | Get API key by ID |
| ListApiKeys | `ListApiKeysRequest` | `ListApiKeysResponse` | List API keys with scope/status filtering and pagination |
| UpdateApiKey | `UpdateApiKeyRequest` | `UpdateApiKeyResponse` | Update API key name, description, permissions, expiration |
| DeleteApiKey | `DeleteApiKeyRequest` | `DeleteApiKeyResponse` | Revoke and soft-delete a key |
| RegenerateApiKey | `RegenerateApiKeyRequest` | `RegenerateApiKeyResponse` | Regenerate key value (returns new raw key) |

### ApiKeyQueryService

Key validation and permission checking (consumed by gateways at runtime).

| Method | Request | Response | Description |
|--------|---------|----------|-------------|
| ValidateApiKey | `ValidateApiKeyRequest` | `ValidateApiKeyResponse` | Validate a raw key, returns scope, permissions, and error reason |
| CheckPermission | `CheckPermissionRequest` | `CheckPermissionResponse` | Check if a key has a specific permission |

### ApiKeyUsageService

Usage tracking and statistics.

| Method | Request | Response | Description |
|--------|---------|----------|-------------|
| RecordUsage | `RecordUsageRequest` | `RecordUsageResponse` | Record API key usage event (endpoint, status, IP, user agent, response time) |
| GetUsageStats | `GetUsageStatsRequest` | `GetUsageStatsResponse` | Get aggregated usage statistics for a key over a time range |
| GetUsageHistory | `GetUsageHistoryRequest` | `GetUsageHistoryResponse` | Get paginated usage history for a key over a time range |

## Models

### ApiKeyModel

| Field | Type | Description |
|-------|------|-------------|
| id | `GuidValue` | API key ID |
| name | `string` | Human-readable name |
| key_prefix | `string` | Prefix for identification |
| scope | `ApiKeyScope` | Access scope |
| status | `ApiKeyStatus` | Current status |
| permissions | `repeated string` | Granted permissions |
| description | `StringValue` | Optional description |
| expires_at | `Timestamp` | Optional expiration |
| last_used_at | `Timestamp` | Last usage timestamp |
| created_at | `Timestamp` | Creation timestamp |
| updated_at | `Timestamp` | Last update timestamp |
| created_by | `GuidValue` | Creator user ID |

### ApiKeyUsageModel

| Field | Type | Description |
|-------|------|-------------|
| id | `GuidValue` | Usage record ID |
| api_key_id | `GuidValue` | Parent API key ID |
| endpoint | `string` | Called endpoint |
| status_code | `int32` | Response status code |
| ip_address | `StringValue` | Client IP address |
| user_agent | `StringValue` | Client user agent |
| timestamp | `Timestamp` | Usage timestamp |
| response_time_ms | `int64` | Response time in milliseconds |

### ApiKeyUsageStatsModel

| Field | Type | Description |
|-------|------|-------------|
| api_key_id | `GuidValue` | API key ID |
| total_requests | `int64` | Total request count |
| successful_requests | `int64` | Successful request count |
| failed_requests | `int64` | Failed request count |
| period_start | `Timestamp` | Stats period start |
| period_end | `Timestamp` | Stats period end |

## Proto file locations

```
DKH.ApiManagementService.Contracts/proto/api_management/
├── api/
│   ├── api_key_crud/v1/
│   │   └── api_keys_crud_service.proto
│   ├── api_key_query/v1/
│   │   └── api_key_query_service.proto
│   └── api_key_usage/v1/
│       └── api_key_usage_service.proto
└── models/
    ├── api_key/v1/
    │   └── api_key.proto
    └── api_key_usage/v1/
        └── api_key_usage.proto
```

## C# namespaces

| Proto package | C# namespace |
|---------------|--------------|
| `proto.api_management.api.api_key_crud.v1` | `DKH.ApiManagementService.Contracts.ApiManagement.Api.ApiKeyCrud.v1` |
| `proto.api_management.api.api_key_query.v1` | `DKH.ApiManagementService.Contracts.ApiManagement.Api.ApiKeyQuery.v1` |
| `proto.api_management.api.api_key_usage.v1` | `DKH.ApiManagementService.Contracts.ApiManagement.Api.ApiKeyUsage.v1` |
| `proto.api_management.models.api_key.v1` | `DKH.ApiManagementService.Contracts.ApiManagement.Models.ApiKey.v1` |
| `proto.api_management.models.api_key_usage.v1` | `DKH.ApiManagementService.Contracts.ApiManagement.Models.ApiKeyUsage.v1` |

## Port

- gRPC: `5012` (HTTP/2)
- Docker internal: `5012`
- Docker external DB: `5212` (PostgreSQL)

*Last updated: February 2026*
