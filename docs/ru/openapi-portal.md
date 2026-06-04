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
