# Доменная модель

> Translation Pending — see [English version](../domain-model.md)

## Сущности

### ApiKeyEntity (корень агрегата)

Наследует `FullAuditedEntityWithKey<Guid>` (предоставляет `Id`, `CreationTime`, `CreatorId`, `LastModificationTime`, `LastModifierId`, `IsDeleted`, `DeleterId`, `DeletionTime`).

| Свойство | Тип | Описание |
|----------|-----|----------|
| Id | `Guid` | Первичный ключ (из базового класса) |
| Name | `string` | Человекочитаемое имя ключа |
| KeyHash | `string` | SHA-256 хеш сырого ключа |
| KeyPrefix | `string` | Первые символы сырого ключа (для идентификации) |
| Scope | `ApiKeyScope` | Область доступа |
| Status | `ApiKeyStatus` | Текущий статус |
| Permissions | `List<string>` | Предоставленные разрешения |
| Description | `string?` | Необязательное описание |
| ExpiresAt | `DateTimeOffset?` | Необязательный срок действия |
| LastUsedAt | `DateTimeOffset?` | Время последнего использования |
| UsageRecords | `IReadOnlyCollection<ApiKeyUsageEntity>` | Навигация к записям использования |

**Методы поведения:**
- `Create(name, keyHash, keyPrefix, scope, permissions, description?, expiresAt?)` — фабричный метод, вызывает `ApiKeyCreatedDomainEvent`
- `Update(name?, description?, permissions?, expiresAt?)` — обновление изменяемых полей
- `Revoke()` — установка статуса Revoked, вызывает `ApiKeyRevokedDomainEvent`
- `Regenerate(newKeyHash, newKeyPrefix)` — замена хеша/префикса ключа, вызывает `ApiKeyRegeneratedDomainEvent`
- `RecordUsage()` — обновление `LastUsedAt` текущим временем UTC
- `IsExpired()` — проверка истечения срока действия
- `IsActive()` — проверка: статус Active и не истёк
- `HasPermission(permission)` — проверка списка разрешений (без учёта регистра)

### ApiKeyUsageEntity

Наследует `Entity<Guid>` (предоставляет `Id`).

| Свойство | Тип | Описание |
|----------|-----|----------|
| Id | `Guid` | Первичный ключ (из базового класса) |
| ApiKeyId | `Guid` | Внешний ключ к ApiKey |
| Endpoint | `string` | Вызванный эндпоинт |
| StatusCode | `int` | Код статуса ответа |
| IpAddress | `string?` | IP-адрес клиента |
| UserAgent | `string?` | User-agent строка клиента |
| Timestamp | `DateTimeOffset` | Время использования |
| ResponseTimeMs | `long` | Время ответа в миллисекундах |
| ApiKey | `ApiKeyEntity` | Навигационное свойство к родительскому ключу |

**Фабричный метод:**
- `Create(apiKeyId, endpoint, statusCode, ipAddress?, userAgent?, responseTimeMs)` — создание записи использования с текущей меткой времени UTC

## Объекты-значения

### ApiKeyHash

Инкапсулирует SHA-256 хеширование сырых API-ключей.

- `FromRawKey(rawKey)` — вычисляет SHA-256 хеш из строки сырого ключа
- `FromHash(hash)` — оборачивает существующий hex-кодированный хеш
- `Value` — hex-кодированная строка хеша (в нижнем регистре)

## Перечисления

### ApiKeyScope

| Значение | Код | Описание |
|----------|-----|----------|
| Mcp | `mcp` | Доступ через MCP-шлюз |
| Webhook | `wh` | Интеграции через вебхуки |
| Partner | `ptr` | Доступ партнёрского API |
| Storefront | `sf` | Доступ API витрины |
| Internal | `int` | Внутренняя коммуникация между сервисами |

### ApiKeyStatus

| Значение | Описание |
|----------|----------|
| Active | Ключ активен и может использоваться |
| Revoked | Ключ был отозван вручную |
| Expired | Ключ просрочен |

## Доменные события

### ApiKeyCreatedDomainEvent

Вызывается при создании нового API-ключа.

| Свойство | Тип | Описание |
|----------|-----|----------|
| ApiKeyId | `Guid` | ID созданного ключа |
| Name | `string` | Имя ключа |
| Scope | `ApiKeyScope` | Область действия ключа |
| OccurredOnUtc | `DateTime` | Время события |

### ApiKeyRevokedDomainEvent

Вызывается при отзыве API-ключа.

| Свойство | Тип | Описание |
|----------|-----|----------|
| ApiKeyId | `Guid` | ID отозванного ключа |
| Name | `string` | Имя ключа |
| OccurredOnUtc | `DateTime` | Время события |

### ApiKeyRegeneratedDomainEvent

Вызывается при перегенерации API-ключа.

| Свойство | Тип | Описание |
|----------|-----|----------|
| ApiKeyId | `Guid` | ID перегенерированного ключа |
| Name | `string` | Имя ключа |
| OccurredOnUtc | `DateTime` | Время события |

## Формат API-ключа

Паттерн: `dkh_{scope}_{random32}`

Примеры:
- `dkh_mcp_a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6`
- `dkh_wh_x9y8z7w6v5u4t3s2r1q0p9o8n7m6l5k4`

Сырой ключ возвращается один раз при создании. Хранится только SHA-256 хеш.
