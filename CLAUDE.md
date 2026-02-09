# CLAUDE.md

## Required Reading (MUST read before working)

Before starting any task in this repository, you MUST read these files from DKH.Architecture:

1. **[AGENTS.md](https://github.com/GZDKH/DKH.Architecture/blob/main/AGENTS.md)** — baseline rules for all repos
2. **[agents-dotnet.md](https://github.com/GZDKH/DKH.Architecture/blob/main/docs/agents-dotnet.md)** — .NET specific rules
3. **[github-workflow.md](https://github.com/GZDKH/DKH.Architecture/blob/main/docs/github-workflow.md)** — GitHub Issues & Project Board

---

This file provides guidance to Claude Code when working in this repository.

> **Baseline rules**: See `AGENTS.md` for unified GZDKH rules (SOLID, DDD, commits, code style, quality guardrails). This file adds service-specific context only.

## Project Overview

DKH.ApiManagementService is a .NET 10 microservice for managing API keys, permissions, and usage tracking across the DKH ecosystem.

- **Framework**: .NET 10.0
- **Architecture**: Clean Architecture + CQRS (MediatR)

## Build Commands

```bash
dotnet restore
dotnet build -c Release
dotnet test
dotnet format --verify-no-changes
dotnet run --project DKH.ApiManagementService.Api

# Apply migrations
dotnet ef database update \
  --project DKH.ApiManagementService.Infrastructure \
  --startup-project DKH.ApiManagementService.Api
```

## Architecture

**Project Structure:**
- `DKH.ApiManagementService.Domain` — Entities (ApiKey, ApiKeyUsage), value objects (ApiKeyHash, Permission), enums (ApiKeyScope)
- `DKH.ApiManagementService.Application` — MediatR handlers, validators, mappers
- `DKH.ApiManagementService.Infrastructure` — EF Core DbContext, repositories, ApiKeyGenerator
- `DKH.ApiManagementService.Api` — gRPC services (3 services), Program.cs, DI setup
- `DKH.ApiManagementService.Contracts` — Protobuf schemas in `proto/api_management/`

**Key Patterns:**
- CQRS via MediatR (Commands, Queries, Handlers per feature)
- FluentValidation for all commands and queries
- ApiKey as aggregate root with ApiKeyUsage child
- Cryptographically secure key generation (SHA-256 hashing)
- Permission-based access control per API key
- Scoped keys: mcp, webhook, partner, storefront, internal

**gRPC Services:**
- `ApiKeyCrudService` — CRUD operations (create, update, delete, get, list, regenerate)
- `ApiKeyValidationService` — Validate keys and check permissions (consumed by McpGateway, future services)
- `ApiKeyUsageService` — Usage statistics and recording

## Contracts (gRPC)

Proto files in `DKH.ApiManagementService.Contracts/proto/api_management/{services|models}/v1/`
- Services: `api_key_crud_service.proto`, `api_key_validation_service.proto`, `api_key_usage_service.proto`
- Models: `api_key.proto`, `api_key_usage.proto`

## Configuration

- `ConnectionStrings:Default` — PostgreSQL `dkh_api_management`
- Port: gRPC `5012`

## API Key Format

- Pattern: `dkh_{scope}_{random32}`
- Scopes: `mcp`, `wh` (webhook), `ptr` (partner), `sf` (storefront), `int` (internal)
- Example: `dkh_mcp_a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6`
- Storage: SHA-256 hash only (raw key never stored)

## External Dependencies

- PostgreSQL via EF Core 10
- DKH.Platform shared libraries
- gRPC consumers: AdminGateway, McpGateway, future services
