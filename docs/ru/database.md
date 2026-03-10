# База данных

> Translation Pending — see [English version](../database.md)

## Подключение

- База данных: `dkh_api_management`
- Провайдер: PostgreSQL 18 (через DKH.Platform.EntityFrameworkCore.PostgreSQL)
- Версия EF Core: 10.0.2
- Ключ строки подключения: `ConnectionStrings:Default`

## Схема

### api_keys

| Колонка | Тип | Ограничения |
|---------|-----|-------------|
| Id | `uuid` | PK |
| Name | `character varying(256)` | NOT NULL |
| KeyHash | `character varying(64)` | NOT NULL, UNIQUE |
| KeyPrefix | `character varying(48)` | NOT NULL |
| Scope | `character varying(32)` | NOT NULL (enum как строка) |
| Status | `character varying(32)` | NOT NULL (enum как строка) |
| Permissions | `jsonb` | NOT NULL |
| Description | `character varying(1024)` | NULLABLE |
| ExpiresAt | `timestamptz` | NULLABLE |
| LastUsedAt | `timestamptz` | NULLABLE |
| CreationTime | `timestamptz` | NOT NULL (из FullAuditedEntity) |
| CreatorId | `uuid` | NULLABLE (из FullAuditedEntity) |
| LastModificationTime | `timestamptz` | NULLABLE (из FullAuditedEntity) |
| LastModifierId | `uuid` | NULLABLE (из FullAuditedEntity) |
| IsDeleted | `boolean` | NOT NULL (из FullAuditedEntity) |
| DeleterId | `uuid` | NULLABLE (из FullAuditedEntity) |
| DeletionTime | `timestamptz` | NULLABLE (из FullAuditedEntity) |

**Индексы:**
- `IX_api_keys_KeyHash` — уникальный индекс по `KeyHash`
- `IX_api_keys_Scope` — индекс по `Scope`
- `IX_api_keys_Status` — индекс по `Status`

### api_key_usage

| Колонка | Тип | Ограничения |
|---------|-----|-------------|
| Id | `uuid` | PK |
| ApiKeyId | `uuid` | FK -> api_keys, NOT NULL |
| Endpoint | `character varying(512)` | NOT NULL |
| StatusCode | `integer` | NOT NULL |
| IpAddress | `character varying(45)` | NULLABLE |
| UserAgent | `character varying(512)` | NULLABLE |
| Timestamp | `timestamptz` | NOT NULL |
| ResponseTimeMs | `bigint` | NOT NULL |

**Индексы:**
- `IX_api_key_usage_ApiKeyId` — индекс FK
- `IX_api_key_usage_Timestamp` — индекс по `Timestamp`

**Внешние ключи:**
- `FK_api_key_usage_api_keys_ApiKeyId` -> `api_keys(Id)` ON DELETE CASCADE

## Миграции

| Миграция | Дата | Описание |
|----------|------|----------|
| `202502100001_InitialCreate` | 2026-02-10 | Создание таблиц `api_keys` и `api_key_usage` |
| `RemoveDuplicateCreatedBy` | 2026-02-15 | Удаление дублирующей колонки `CreatedBy` из `api_keys` |

```bash
# Добавить миграцию
dotnet ef migrations add <Name> \
  --startup-project DKH.ApiManagementService.Api \
  --project DKH.ApiManagementService.Infrastructure

# Применить миграции
dotnet ef database update \
  --startup-project DKH.ApiManagementService.Api \
  --project DKH.ApiManagementService.Infrastructure
```
