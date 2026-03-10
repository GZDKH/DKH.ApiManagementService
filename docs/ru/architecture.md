# Архитектура

> Translation Pending — see [English version](../architecture.md)

## Обзор

DKH.ApiManagementService — микросервис на .NET 10, отвечающий за управление жизненным циклом API-ключей, контроль разрешений и отслеживание использования в экосистеме DKH.

## Стиль архитектуры

Clean Architecture с CQRS через MediatR.

```
Api (gRPC) → Application (Commands/Queries) → Domain (Entities) ← Infrastructure (EF Core)
```

## Слои

### Domain

- **Сущности**: `ApiKeyEntity` (корень агрегата), `ApiKeyUsageEntity`
- **Объекты-значения**: `ApiKeyHash` (SHA-256)
- **Перечисления**: `ApiKeyScope`, `ApiKeyStatus`
- Без зависимостей от инфраструктуры

### Application

- Обработчики команд и запросов MediatR
- Валидаторы FluentValidation для всех команд/запросов
- Абстракции: `IAppDbContext`, `IApiKeyRepository`, `IApiKeyUsageRepository`, `IApiKeyGenerator`
- Маперы: преобразования domain-to-proto и proto-to-domain

### Infrastructure

- EF Core с PostgreSQL (через DKH.Platform)
- Реализации репозиториев
- `ApiKeyGenerator`: криптографически безопасная генерация ключей через `RandomNumberGenerator`

### Api

- Три gRPC-сервиса, диспетчеризующие запросы в MediatR
- Точка входа через Platform builder (`Platform.CreateWeb`)

## Взаимодействие

```
AdminGateway (REST) ──gRPC──► ApiManagementService ──► PostgreSQL
McpGateway   (MCP)  ──gRPC──► ApiManagementService
```

## Конфигурация

Все настройки следуют конвенциям DKH.Platform:

- `ConnectionStrings:Default` — подключение к PostgreSQL
- `Platform:Logging` — конфигурация Serilog
- `Platform:Grpc` — настройки gRPC-сервера
- Порт: `5012` (HTTP/2)
