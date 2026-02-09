using DKH.ApiManagementService.Domain.Enums;
using DKH.Platform.Domain.Events;

namespace DKH.ApiManagementService.Domain.Entities;

public class ApiKeyEntity : AggregateRoot<Guid>
{
    private readonly List<ApiKeyUsageEntity> _usageRecords = [];

    private ApiKeyEntity()
    {
    }

    public ApiKeyEntity(
        string name,
        string keyHash,
        string keyPrefix,
        ApiKeyScope scope,
        IReadOnlyList<string> permissions,
        string createdBy,
        string? description = null,
        DateTimeOffset? expiresAt = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        KeyHash = keyHash;
        KeyPrefix = keyPrefix;
        Scope = scope;
        Status = ApiKeyStatus.Active;
        Permissions = [.. permissions];
        CreatedBy = createdBy;
        Description = description;
        ExpiresAt = expiresAt;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public string Name { get; private set; } = null!;

    public string KeyHash { get; private set; } = null!;

    public string KeyPrefix { get; private set; } = null!;

    public ApiKeyScope Scope { get; private set; }

    public ApiKeyStatus Status { get; private set; }

    public List<string> Permissions { get; private set; } = [];

    public string? Description { get; private set; }

    public string CreatedBy { get; private set; } = null!;

    public DateTimeOffset? ExpiresAt { get; private set; }

    public DateTimeOffset? LastUsedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<ApiKeyUsageEntity> UsageRecords => _usageRecords.AsReadOnly();

    public void Update(string? name, string? description, IReadOnlyList<string>? permissions, DateTimeOffset? expiresAt)
    {
        if (name is not null)
        {
            Name = name;
        }

        if (description is not null)
        {
            Description = description;
        }

        if (permissions is not null)
        {
            Permissions = [.. permissions];
        }

        if (expiresAt is not null)
        {
            ExpiresAt = expiresAt;
        }

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Revoke()
    {
        Status = ApiKeyStatus.Revoked;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Regenerate(string newKeyHash, string newKeyPrefix)
    {
        KeyHash = newKeyHash;
        KeyPrefix = newKeyPrefix;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RecordUsage()
    {
        LastUsedAt = DateTimeOffset.UtcNow;
    }

    public bool IsExpired() => ExpiresAt.HasValue && ExpiresAt.Value <= DateTimeOffset.UtcNow;

    public bool IsActive() => Status == ApiKeyStatus.Active && !IsExpired();

    public bool HasPermission(string permission) => Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
}
