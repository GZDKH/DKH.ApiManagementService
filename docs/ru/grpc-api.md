# gRPC API

> Translation Pending — see [English version](../grpc-api.md)

## Сервисы

### ApiKeysCrudService

CRUD-операции для управления API-ключами.

| Метод | Запрос | Ответ | Описание |
|-------|--------|-------|----------|
| CreateApiKey | `CreateApiKeyRequest` | `CreateApiKeyResponse` | Создание нового API-ключа (возвращает сырой ключ один раз) |
| GetApiKey | `GetApiKeyRequest` | `GetApiKeyResponse` | Получение API-ключа по ID |
| ListApiKeys | `ListApiKeysRequest` | `ListApiKeysResponse` | Список API-ключей с фильтрацией по scope/status/customer/environment/tier и пагинацией |
| UpdateApiKey | `UpdateApiKeyRequest` | `UpdateApiKeyResponse` | Обновление имени, описания, разрешений, срока действия, customer, environment и tier |
| DeleteApiKey | `DeleteApiKeyRequest` | `DeleteApiKeyResponse` | Отзыв и мягкое удаление ключа |
| RegenerateApiKey | `RegenerateApiKeyRequest` | `RegenerateApiKeyResponse` | Перегенерация значения ключа (возвращает новый сырой ключ) |

### ApiKeyQueryService

Валидация ключей и проверка разрешений (используется шлюзами в рантайме).

| Метод | Запрос | Ответ | Описание |
|-------|--------|-------|----------|
| ValidateApiKey | `ValidateApiKeyRequest` | `ValidateApiKeyResponse` | Валидация сырого ключа, возвращает scope, разрешения, customer, environment, rate-limit tier и причину ошибки |
| CheckPermission | `CheckPermissionRequest` | `CheckPermissionResponse` | Проверка наличия определённого разрешения у ключа |

### ApiKeyUsageService

Отслеживание использования и статистика.

| Метод | Запрос | Ответ | Описание |
|-------|--------|-------|----------|
| RecordUsage | `RecordUsageRequest` | `RecordUsageResponse` | Запись события использования API-ключа (эндпоинт, статус, IP, user agent, время ответа) |
| GetUsageStats | `GetUsageStatsRequest` | `GetUsageStatsResponse` | Агрегированная статистика использования ключа за период |
| GetUsageHistory | `GetUsageHistoryRequest` | `GetUsageHistoryResponse` | Пагинированная история использования ключа за период |

## Модели

### ApiKeyModel

| Поле | Тип | Описание |
|------|-----|----------|
| id | `GuidValue` | ID API-ключа |
| name | `string` | Человекочитаемое имя |
| key_prefix | `string` | Префикс для идентификации |
| scope | `ApiKeyScope` | Область доступа |
| status | `ApiKeyStatus` | Текущий статус |
| permissions | `repeated string` | Предоставленные разрешения |
| description | `StringValue` | Необязательное описание |
| expires_at | `Timestamp` | Необязательный срок действия |
| last_used_at | `Timestamp` | Время последнего использования |
| created_at | `Timestamp` | Время создания |
| updated_at | `Timestamp` | Время последнего обновления |
| created_by | `GuidValue` | ID пользователя-создателя |
| customer_id | `GuidValue` | Владелец customer/tenant для публичных ключей |
| environment | `ApiKeyEnvironment` | Sandbox или production |
| rate_limit_tier | `ApiKeyRateLimitTier` | Настроенный tier |
| rate_limit_requests_per_minute | `int32` | Рассчитанный лимит запросов в минуту |
| last_rotated_at | `Timestamp` | Время последней ротации |
| rotation_count | `int32` | Количество выполненных ротаций |
| previous_key_prefix | `StringValue` | Предыдущий префикс ключа после ротации |

### ApiKeyUsageModel

| Поле | Тип | Описание |
|------|-----|----------|
| id | `GuidValue` | ID записи использования |
| api_key_id | `GuidValue` | ID родительского API-ключа |
| endpoint | `string` | Вызванный эндпоинт |
| status_code | `int32` | Код статуса ответа |
| ip_address | `StringValue` | IP-адрес клиента |
| user_agent | `StringValue` | User-agent клиента |
| timestamp | `Timestamp` | Время использования |
| response_time_ms | `int64` | Время ответа в миллисекундах |
| customer_id | `GuidValue` | Владелец customer/tenant, скопированный из ключа при записи |
| environment | `ApiKeyEnvironment` | Окружение ключа при записи |
| rate_limit_tier | `ApiKeyRateLimitTier` | Tier при записи |
| rate_limit_requests_per_minute | `int32` | Рассчитанный лимит при записи |

### ApiKeyUsageStatsModel

| Поле | Тип | Описание |
|------|-----|----------|
| api_key_id | `GuidValue` | ID API-ключа |
| total_requests | `int64` | Общее количество запросов |
| successful_requests | `int64` | Количество успешных запросов |
| failed_requests | `int64` | Количество неуспешных запросов |
| period_start | `Timestamp` | Начало периода статистики |
| period_end | `Timestamp` | Конец периода статистики |

## Расположение proto-файлов

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

## Пространства имён C#

| Proto-пакет | Пространство имён C# |
|-------------|----------------------|
| `proto.api_management.api.api_key_crud.v1` | `DKH.ApiManagementService.Contracts.ApiManagement.Api.ApiKeyCrud.v1` |
| `proto.api_management.api.api_key_query.v1` | `DKH.ApiManagementService.Contracts.ApiManagement.Api.ApiKeyQuery.v1` |
| `proto.api_management.api.api_key_usage.v1` | `DKH.ApiManagementService.Contracts.ApiManagement.Api.ApiKeyUsage.v1` |
| `proto.api_management.models.api_key.v1` | `DKH.ApiManagementService.Contracts.ApiManagement.Models.ApiKey.v1` |
| `proto.api_management.models.api_key_usage.v1` | `DKH.ApiManagementService.Contracts.ApiManagement.Models.ApiKeyUsage.v1` |

## Порт

- gRPC: `5012` (HTTP/2)
- Docker внутренний: `5012`
- Docker внешний (БД): `5212` (PostgreSQL)
