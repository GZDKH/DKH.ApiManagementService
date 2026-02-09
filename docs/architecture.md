# Architecture

## Overview

DKH.ApiManagementService is a .NET 10 microservice responsible for API key lifecycle management, permission enforcement, and usage tracking across the DKH ecosystem.

## Architecture style

Clean Architecture with CQRS via MediatR.

```
Api (gRPC) → Application (Commands/Queries) → Domain (Entities) ← Infrastructure (EF Core)
```

## Layers

### Domain

- **Entities**: `ApiKeyEntity` (aggregate root), `ApiKeyUsageEntity`
- **Value objects**: `ApiKeyHash` (SHA-256)
- **Enums**: `ApiKeyScope`, `ApiKeyStatus`
- No infrastructure dependencies

### Application

- MediatR command and query handlers
- FluentValidation validators for all commands/queries
- Abstractions: `IAppDbContext`, `IApiKeyRepository`, `IApiKeyUsageRepository`, `IApiKeyGenerator`
- Mappers: domain-to-proto and proto-to-domain conversions

### Infrastructure

- EF Core with PostgreSQL (via DKH.Platform)
- Repository implementations
- `ApiKeyGenerator`: cryptographically secure key generation using `RandomNumberGenerator`

### Api

- Three gRPC services dispatching to MediatR
- Platform builder entry point (`Platform.CreateWeb`)

## Communication

```
AdminGateway (REST) ──gRPC──► ApiManagementService ──► PostgreSQL
McpGateway   (MCP)  ──gRPC──► ApiManagementService
```

## Configuration

All settings follow DKH.Platform conventions:

- `ConnectionStrings:Default` — PostgreSQL connection
- `Platform:Logging` — Serilog configuration
- `Platform:Grpc` — gRPC server options
- Port: `5012` (HTTP/2)
