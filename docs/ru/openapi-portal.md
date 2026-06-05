# OpenAPI Portal

`DKH.ApiManagementService` публикует небольшой developer portal для слоя
управления публичным API.

## Локальные маршруты

| Маршрут | Назначение |
|---------|------------|
| `/openapi/api-management.json` | Сгенерированный OpenAPI документ для группы `api-management` |
| `/scalar` | UI справочника API через Scalar |
| `/swagger` | Swagger UI |
| `/api/v1/developer-portal/documents` | Машиночитаемый индекс опубликованных документов и UI маршрутов |
| `/api/v1/webhook-subscriptions` | Создание и список partner webhook subscriptions |
| `/api/v1/webhook-subscriptions/{id}` | Получение или обновление webhook subscription |
| `/api/v1/webhook-subscriptions/{id}/disable` | Отключение подписки без удаления |
| `/api/v1/webhook-subscriptions/{id}/rotate-secret` | Ротация metadata HMAC signing secret |

Сервис слушает порт `5012`. Kestrel настроен на `Http1AndHttp2`, чтобы один
endpoint обслуживал браузерную документацию и существующих gRPC клиентов.

## Production hosting

В production домен `developers.dkh.<domain>` должен маршрутизироваться на те
же application routes через edge proxy. Приложение сохраняет стабильные
префиксы (`/openapi`, `/scalar`, `/swagger`), поэтому reverse-proxy hostname
может меняться без изменения сгенерированных клиентов и ссылок документации.

## Конфигурация

Портал использует стандартную секцию Platform OpenAPI:

```text
Platform:Http:OpenApi
```

Override через переменные окружения задаются обычным форматом с двойным
подчеркиванием, например
`Platform__Http__OpenApi__Documents__0__Name=api-management`.

## Webhook subscriptions

Webhook endpoints требуют authenticated API key principal с claim `api_key_id`
и permission `webhooks:subscribe` или `webhooks.manage`. Controller
ограничивает list/get/update/disable/rotate значениями `api_key_id` и
optional `customer_id`, которые выдаёт `ApiKeyValidator`.

Create/update requests сохраняют callback URL, нормализованные events,
retry policy, DLQ flag и HMAC signing secret metadata. Raw signing secret
принимается в request, но не хранится: в БД остаются только SHA-256 hash и
display prefix. Delivery workers используют поля observability
(`LastDeliveryAt`, `LastDeliverySucceeded`, `LastDeliveryStatusCode`,
`LastDeliveryError`, `FailureCount`) для мониторинга подписчика.
