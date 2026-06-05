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
| CustomerId | `uuid` | NULLABLE, владелец customer/tenant для публичных API-ключей |
| Environment | `character varying(32)` | NOT NULL, по умолчанию `Production` |
| RateLimitTier | `character varying(32)` | NOT NULL, по умолчанию `Standard` |
| RateLimitRequestsPerMinute | `integer` | NOT NULL, по умолчанию `600` |
| Description | `character varying(1024)` | NULLABLE |
| ExpiresAt | `timestamptz` | NULLABLE |
| LastUsedAt | `timestamptz` | NULLABLE |
| LastRotatedAt | `timestamptz` | NULLABLE |
| PreviousKeyPrefix | `character varying(48)` | NULLABLE |
| RotationCount | `integer` | NOT NULL, по умолчанию `0` |
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
- `IX_api_keys_CustomerId` — индекс по `CustomerId`
- `IX_api_keys_Environment` — индекс по `Environment`
- `IX_api_keys_RateLimitTier` — индекс по `RateLimitTier`
- `IX_api_keys_CustomerId_Environment` — составной индекс для управления ключами customer/environment

### api_key_usage

| Колонка | Тип | Ограничения |
|---------|-----|-------------|
| Id | `uuid` | PK |
| ApiKeyId | `uuid` | FK -> api_keys, NOT NULL |
| Endpoint | `character varying(512)` | NOT NULL |
| StatusCode | `integer` | NOT NULL |
| IpAddress | `character varying(45)` | NULLABLE |
| UserAgent | `character varying(512)` | NULLABLE |
| CustomerId | `uuid` | NULLABLE, копируется из API-ключа при записи |
| Environment | `character varying(32)` | NOT NULL, копируется из API-ключа при записи |
| RateLimitTier | `character varying(32)` | NOT NULL, копируется из API-ключа при записи |
| RateLimitRequestsPerMinute | `integer` | NOT NULL, копируется из API-ключа при записи |
| Timestamp | `timestamptz` | NOT NULL |
| ResponseTimeMs | `bigint` | NOT NULL |

**Индексы:**
- `IX_api_key_usage_ApiKeyId` — индекс FK
- `IX_api_key_usage_Timestamp` — индекс по `Timestamp`
- `IX_api_key_usage_CustomerId` — индекс по `CustomerId`
- `IX_api_key_usage_Environment` — индекс по `Environment`
- `IX_api_key_usage_RateLimitTier` — индекс по `RateLimitTier`
- `IX_api_key_usage_CustomerId_Environment` — составной индекс для аналитики customer/environment

**Внешние ключи:**
- `FK_api_key_usage_api_keys_ApiKeyId` -> `api_keys(Id)` ON DELETE CASCADE

### webhook_subscriptions

| Колонка | Тип | Ограничения |
|---------|-----|-------------|
| Id | `uuid` | PK |
| ApiKeyId | `uuid` | NULLABLE, API-ключ-владелец подписки |
| CustomerId | `uuid` | NULLABLE, владелец customer/tenant |
| Name | `character varying(256)` | NOT NULL |
| CallbackUrl | `character varying(2048)` | NOT NULL |
| Events | `jsonb` | NOT NULL, нормализованные имена событий |
| SigningSecretHash | `character varying(64)` | NOT NULL, SHA-256 hash signing secret |
| SigningSecretPrefix | `character varying(32)` | NOT NULL, только отображаемый prefix |
| Status | `character varying(32)` | NOT NULL (`Active`, `Disabled`) |
| RetryMaxAttempts | `integer` | NOT NULL, default `5` |
| RetryBackoffSeconds | `integer` | NOT NULL, default `30` |
| DlqEnabled | `boolean` | NOT NULL, default `true` |
| LastDeliveryAt | `timestamptz` | NULLABLE |
| LastDeliverySucceeded | `boolean` | NULLABLE |
| LastDeliveryStatusCode | `integer` | NULLABLE |
| LastDeliveryError | `character varying(2048)` | NULLABLE |
| FailureCount | `integer` | NOT NULL, default `0` |
| LastRotatedAt | `timestamptz` | NULLABLE |
| RotationCount | `integer` | NOT NULL, default `0` |

**Индексы:**
- `IX_webhook_subscriptions_ApiKeyId`
- `IX_webhook_subscriptions_CustomerId`
- `IX_webhook_subscriptions_Status`
- `IX_webhook_subscriptions_CustomerId_Status`

## Миграции

| Миграция | Дата | Описание |
|----------|------|----------|
| `202502100001_InitialCreate` | 2026-02-10 | Создание таблиц `api_keys` и `api_key_usage` |
| `RemoveDuplicateCreatedBy` | 2026-02-15 | Удаление дублирующей колонки `CreatedBy` из `api_keys` |
| `AddPublicApiHardening` | 2026-06-04 | Добавляет владельца customer, sandbox/production environment, rate-limit tier, метаданные ротации и измерения usage analytics |
| `AddWebhookSubscriptions` | 2026-06-05 | Добавляет `webhook_subscriptions` для callback URL, событий, HMAC metadata, retry/DLQ policy и delivery observability |

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
