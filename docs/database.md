# Database

## Connection

- Database: `dkh_api_management`
- Provider: PostgreSQL 18 (via DKH.Platform.EntityFrameworkCore.PostgreSQL)
- Connection string key: `ConnectionStrings:Default`

## Schema

### api_keys

| Column | Type | Constraints |
|--------|------|-------------|
| id | `uuid` | PK |
| name | `varchar(200)` | NOT NULL |
| description | `varchar(500)` | NULLABLE |
| key_hash | `varchar(128)` | NOT NULL, UNIQUE |
| key_prefix | `varchar(20)` | NOT NULL |
| scope | `varchar(50)` | NOT NULL (enum as string) |
| status | `varchar(50)` | NOT NULL (enum as string) |
| permissions | `jsonb` | NOT NULL |
| expires_at | `timestamptz` | NULLABLE |
| last_used_at | `timestamptz` | NULLABLE |
| total_requests | `bigint` | NOT NULL, DEFAULT 0 |
| created_at | `timestamptz` | NOT NULL |
| updated_at | `timestamptz` | NULLABLE |

**Indexes:**
- `ix_api_keys_key_hash` — unique index on `key_hash`
- `ix_api_keys_scope` — index on `scope`
- `ix_api_keys_status` — index on `status`

### api_key_usage_records

| Column | Type | Constraints |
|--------|------|-------------|
| id | `uuid` | PK |
| api_key_id | `uuid` | FK → api_keys, NOT NULL |
| endpoint | `varchar(500)` | NOT NULL |
| ip_address | `varchar(45)` | NULLABLE |
| status_code | `integer` | NOT NULL |
| timestamp | `timestamptz` | NOT NULL |

**Indexes:**
- `ix_api_key_usage_records_api_key_id` — FK index
- `ix_api_key_usage_records_timestamp` — index on `timestamp`

## Migrations

```bash
# Add migration
dotnet ef migrations add <Name> \
  --startup-project DKH.ApiManagementService.Api \
  --project DKH.ApiManagementService.Infrastructure

# Apply migrations
dotnet ef database update \
  --startup-project DKH.ApiManagementService.Api \
  --project DKH.ApiManagementService.Infrastructure
```
