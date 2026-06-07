# Module manifest exemplars

Canonical `module.json` / `edition.json` examples for the modular-platform foundation
(ADR-053, Layer 0). They are the reference templates the per-service `module.json` sweep
([#278] checklist, bead `dkh-4xlnx`) copies from, and they are validated end-to-end by
`ModuleManifestExemplarsTests` against the real ingestion path
(`DirectoryModuleManifestSource` → `PlatformModuleDependencyResolver`).

These are **fixtures, not runtime seed data**: the production catalog reflects actually
deployed components (each service ships its own `module.json`, collected into
`Modularity:ManifestsDirectory` at deploy time). Nothing here is loaded by the running
service.

## Components

| id | kind | provides | requires | entitlement |
|----|------|----------|----------|-------------|
| `dkh.product-catalog` | Service | `catalog.products`, `catalog.search` | — | (core, always available) |
| `dkh.payments` | Service | `payments.checkout` | `catalog.products [3.0.0,)` | `payments` |
| `dkh.logistics` | Service | `logistics.rates`, `logistics.routes` | `catalog.products` | `logistics` |
| `dkh.engagement` | Service | `engagement.requests`, `engagement.providers`, `engagement.reports` | `catalog.products` | (core, always available) |
| `dkh.ai.claude` | Plugin | `ai.provider.claude` | — | `ai.assistant` |
| `dkh.payments.stripe` | Plugin | `payments.provider.stripe` | `payments.checkout [1.0.0,)` | `payments.stripe` |
| `dkh.payments.telegram` | Plugin | `payments.provider.telegram` | `payments.checkout` | `payments.telegram` |

### Activation order (topological)

```
dkh.product-catalog, dkh.ai.claude   (no dependencies)
  └─ dkh.payments, dkh.logistics, dkh.engagement   (need catalog.products)
       └─ dkh.payments.stripe, dkh.payments.telegram   (need payments.checkout)
```

Every `requires` capability is satisfied within the set, so the resolver returns
`IsSuccessful == true` with no missing-capability, version-conflict, or cycle problems.

## Editions

| id | components | entitlement |
|----|-----------|-------------|
| `commerce-suite` | product-catalog + payments + stripe + telegram | `commerce-suite` |
| `logistics-suite` | product-catalog + logistics | `logistics-suite` |

## Schema

`module.json` → `PlatformModuleManifest`, `edition.json` → `PlatformEditionManifest`
(both in `DKH.Platform.Modularity`). JSON is read with `JsonSerializerDefaults.Web`
(camelCase, case-insensitive); `kind` is a string enum (`Service` | `Plugin`, default
`Plugin` so a legacy `plugin.json` reads as a plugin component).

[#278]: https://github.com/GZDKH/DKH.Architecture/issues/278
