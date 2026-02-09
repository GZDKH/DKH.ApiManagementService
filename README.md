# DKH.ApiManagementService

API Management service for the DKH ecosystem. Manages API keys, permissions, usage tracking, and rate limiting via gRPC.

## Documentation

- [Architecture Docs (EN)](https://github.com/GZDKH/DKH.Architecture/blob/main/en/services/backend/api-management-index.md)
- [Architecture Docs (RU)](https://github.com/GZDKH/DKH.Architecture/blob/main/ru/services/backend/api-management-index.md)
- [Local Docs](./docs/README.md)

## Quick Start

```bash
# Restore and build
dotnet restore
dotnet build -c Release

# Run
dotnet run --project DKH.ApiManagementService.Api

# Run tests
dotnet test
```

## Port

- gRPC: `5012`
- Database: `dkh_api_management` (PostgreSQL)
