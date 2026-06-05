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
| CustomerId | `Guid?` | Владелец customer/tenant для публичных ключей |
| Environment | `ApiKeyEnvironment` | Sandbox или production окружение ключа |
| RateLimitTier | `ApiKeyRateLimitTier` | Настроенный rate-limit tier |
| RateLimitRequestsPerMinute | `int` | Рассчитанный лимит запросов в минуту |
| Description | `string?` | Необязательное описание |
| ExpiresAt | `DateTimeOffset?` | Необязательный срок действия |
| LastUsedAt | `DateTimeOffset?` | Время последнего использования |
| LastRotatedAt | `DateTimeOffset?` | Время последней ротации |
| PreviousKeyPrefix | `string?` | Предыдущий префикс ключа после ротации |
| RotationCount | `int` | Количество выполненных ротаций |
| UsageRecords | `IReadOnlyCollection<ApiKeyUsageEntity>` | Навигация к записям использования |

**Методы поведения:**
- `Create(name, keyHash, keyPrefix, scope, permissions, customerId?, environment, rateLimitTier, description?, expiresAt?)` — фабричный метод, вызывает `ApiKeyCreatedDomainEvent`
- `Update(name?, description?, permissions?, expiresAt?, customerId?, environment?, rateLimitTier?)` — обновление изменяемых полей и policy-метаданных публичного API
- `Revoke()` — установка статуса Revoked, вызывает `ApiKeyRevokedDomainEvent`
- `Regenerate(newKeyHash, newKeyPrefix)` — замена хеша/префикса ключа, записывает метаданные ротации и вызывает `ApiKeyRegeneratedDomainEvent`
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
| CustomerId | `Guid?` | Владелец customer/tenant, скопированный из ключа при записи |
| Environment | `ApiKeyEnvironment` | Окружение ключа при записи |
| RateLimitTier | `ApiKeyRateLimitTier` | Tier ключа при записи |
| RateLimitRequestsPerMinute | `int` | Рассчитанный лимит при записи |
| Timestamp | `DateTimeOffset` | Время использования |
| ResponseTimeMs | `long` | Время ответа в миллисекундах |
| ApiKey | `ApiKeyEntity` | Навигационное свойство к родительскому ключу |

**Фабричный метод:**
- `Create(apiKeyId, endpoint, statusCode, ipAddress?, userAgent?, responseTimeMs, customerId?, environment, rateLimitTier, rateLimitRequestsPerMinute)` — создание записи использования с текущей меткой времени UTC и аналитическими измерениями

### WebhookSubscriptionEntity (корень агрегата)

Наследует `FullAuditedEntityWithKey<Guid>`.

| Свойство | Тип | Описание |
|----------|-----|----------|
| Id | `Guid` | Первичный ключ |
| ApiKeyId | `Guid?` | API-ключ, которому принадлежит подписка |
| CustomerId | `Guid?` | Владелец customer/tenant для partner-facing subscriptions |
| Name | `string` | Человекочитаемое имя подписки |
| CallbackUrl | `string` | Callback endpoint партнёра |
| Events | `List<string>` | Нормализованные имена событий подписки |
| SigningSecretHash | `string` | SHA-256 hash webhook signing secret |
| SigningSecretPrefix | `string` | Только отображаемый prefix; raw secret не хранится |
| Status | `WebhookSubscriptionStatus` | Lifecycle state |
| RetryMaxAttempts | `int` | Максимальное число retry перед DLQ |
| RetryBackoffSeconds | `int` | Интервал retry в секундах |
| DlqEnabled | `bool` | Разрешён ли DLQ для неуспешных доставок |
| LastDeliveryAt | `DateTimeOffset?` | Время последней попытки доставки |
| LastDeliverySucceeded | `bool?` | Результат последней доставки |
| LastDeliveryStatusCode | `int?` | HTTP status партнёра |
| LastDeliveryError | `string?` | Последняя ошибка доставки |
| FailureCount | `int` | Число последовательных неуспешных доставок |
| LastRotatedAt | `DateTimeOffset?` | Время последней ротации signing secret |
| RotationCount | `int` | Количество ротаций signing secret |

**Методы поведения:**
- `Create(apiKeyId?, customerId?, name, callbackUrl, events, rawSigningSecret, retryMaxAttempts, retryBackoffSeconds, dlqEnabled)` — нормализует events и хранит только hash/prefix signing secret
- `Update(name, callbackUrl, events, retryMaxAttempts, retryBackoffSeconds, dlqEnabled)` — обновляет route и retry/DLQ policy
- `Disable()` — отключает доставку без удаления подписки
- `RotateSecret(rawSigningSecret)` — заменяет hash/prefix и пишет telemetry ротации
- `RecordDelivery(succeeded, statusCode, error, deliveredAt)` — записывает observability поля подписчика

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

### ApiKeyEnvironment

| Значение | Описание |
|----------|----------|
| Sandbox | Непроизводственное тестирование партнёров |
| Production | Боевой партнёрский трафик |

### ApiKeyRateLimitTier

| Значение | Запросов/минуту |
|----------|-----------------|
| Development | 60 |
| Standard | 600 |
| Professional | 3 000 |
| Enterprise | 12 000 |

Sandbox-ключи ограничены 120 запросами/минуту независимо от tier.

### WebhookSubscriptionStatus

| Значение | Описание |
|----------|----------|
| Active | Подписка может получать доставки |
| Disabled | Подписка сохранена, но доставка отключена |

## Доменные сервисы

### WebhookSigningSecretHasher

Вычисляет lowercase SHA-256 hash raw signing secret и display-only prefix. Raw webhook signing secret не хранится.

### WebhookSignatureService

Формирует deterministic HMAC-SHA256 signature по `timestamp.payload` с prefix `sha256=` для outbound webhook delivery.

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
