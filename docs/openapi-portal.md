# OpenAPI Portal

`DKH.ApiManagementService` exposes a small developer portal surface for the
public API management layer.

## Local Routes

| Route | Purpose |
|-------|---------|
| `/openapi/api-management.json` | Generated OpenAPI document for the `api-management` group |
| `/scalar` | Scalar API reference UI |
| `/swagger` | Swagger UI |
| `/api/v1/developer-portal/documents` | Machine-readable index of published documents and UI routes |
| `/api/v1/webhook-subscriptions` | Create/list partner webhook subscriptions |
| `/api/v1/webhook-subscriptions/{id}` | Get or update a webhook subscription |
| `/api/v1/webhook-subscriptions/{id}/disable` | Disable a subscription without deleting it |
| `/api/v1/webhook-subscriptions/{id}/rotate-secret` | Rotate the HMAC signing secret metadata |

The service listens on port `5012`. Kestrel is configured with
`Http1AndHttp2` so the same endpoint can serve browser-based documentation
and existing gRPC callers.

## Production Hosting

Production should route `developers.dkh.<domain>` to the same application
routes through the edge proxy. The application itself keeps route prefixes
stable (`/openapi`, `/scalar`, `/swagger`) so reverse-proxy hostnames can
change without changing generated clients or documentation links.

## Configuration

The portal uses the standard Platform OpenAPI configuration section:

```text
Platform:Http:OpenApi
```

Environment variable overrides use the usual double-underscore form, for
example `Platform__Http__OpenApi__Documents__0__Name=api-management`.

## Webhook Subscriptions

Webhook subscription endpoints require an authenticated API key principal with
an `api_key_id` claim plus either `webhooks:subscribe` or `webhooks.manage`
permission. The controller scopes list/get/update/disable/rotate operations to
the `api_key_id` and optional `customer_id` claims emitted by `ApiKeyValidator`.

Create/update requests store callback URL, normalized event names, retry
policy, DLQ flag, and HMAC signing secret metadata. The raw signing secret is
accepted in the request but never stored; persistence keeps only a SHA-256 hash
and display prefix. Delivery workers can later use the subscription telemetry
fields (`LastDeliveryAt`, `LastDeliverySucceeded`, `LastDeliveryStatusCode`,
`LastDeliveryError`, `FailureCount`) for subscriber observability.
