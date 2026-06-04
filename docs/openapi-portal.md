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
