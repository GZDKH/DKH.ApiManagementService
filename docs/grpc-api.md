# gRPC API

## Services

### ApiKeyCrudService

CRUD operations for API key management.

| Method | Request | Response | Description |
|--------|---------|----------|-------------|
| CreateApiKey | `CreateApiKeyRequest` | `CreateApiKeyResponse` | Create a new API key |
| GetApiKey | `GetApiKeyRequest` | `GetApiKeyResponse` | Get API key by ID |
| ListApiKeys | `ListApiKeysRequest` | `ListApiKeysResponse` | List API keys with filtering |
| UpdateApiKey | `UpdateApiKeyRequest` | `UpdateApiKeyResponse` | Update API key metadata |
| DeleteApiKey | `DeleteApiKeyRequest` | `DeleteApiKeyResponse` | Revoke and soft-delete a key |
| RegenerateApiKey | `RegenerateApiKeyRequest` | `RegenerateApiKeyResponse` | Regenerate key value |

### ApiKeyValidationService

Key validation and permission checking (consumed by gateways).

| Method | Request | Response | Description |
|--------|---------|----------|-------------|
| ValidateApiKey | `ValidateApiKeyRequest` | `ValidateApiKeyResponse` | Validate a raw key, returns scope and permissions |
| CheckPermission | `CheckPermissionRequest` | `CheckPermissionResponse` | Check if a key has a specific permission |

### ApiKeyUsageService

Usage tracking and statistics.

| Method | Request | Response | Description |
|--------|---------|----------|-------------|
| RecordUsage | `RecordUsageRequest` | `RecordUsageResponse` | Record API key usage event |
| GetUsageStats | `GetUsageStatsRequest` | `GetUsageStatsResponse` | Get usage statistics for a key |
| GetUsageHistory | `GetUsageHistoryRequest` | `GetUsageHistoryResponse` | Get paginated usage history |

## Proto file locations

```
DKH.ApiManagementService.Contracts/proto/api_management/
├── models/v1/
│   ├── api_key.proto
│   └── api_key_usage.proto
└── services/v1/
    ├── api_key_crud_service.proto
    ├── api_key_validation_service.proto
    └── api_key_usage_service.proto
```

## Port

- gRPC: `5012` (HTTP/2)
- Docker internal: `5012`
- Docker external DB: `5212` (PostgreSQL)
