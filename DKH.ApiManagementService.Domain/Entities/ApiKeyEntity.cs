using DKH.ApiManagementService.Domain.Entities.Events;
using DKH.ApiManagementService.Domain.Enums;
using DKH.ApiManagementService.Domain.Services;
using DKH.Platform.Domain.Entities.Auditing;
using DKH.Platform.Domain.Events;

namespace DKH.ApiManagementService.Domain.Entities;

public sealed class ApiKeyEntity : FullAuditedEntityWithKey<Guid>, IAggregateRoot
{
    private readonly List<ApiKeyUsageEntity> _usageRecords = [];
    private readonly List<IDomainEvent> _domainEvents = [];

    private ApiKeyEntity()
    {
        Name = string.Empty;
        KeyHash = string.Empty;
        KeyPrefix = string.Empty;
        Environment = ApiKeyEnvironment.Production;
        RateLimitTier = ApiKeyRateLimitTier.Standard;
        RateLimitRequestsPerMinute = ApiKeyRateLimitPolicy.GetRequestsPerMinute(Environment, RateLimitTier);
    }

    private ApiKeyEntity(
        string name,
        string keyHash,
        string keyPrefix,
        ApiKeyScope scope,
        IReadOnlyList<string> permissions,
        Guid? customerId,
        Guid? storefrontId,
        ApiKeyEnvironment environment,
        ApiKeyRateLimitTier rateLimitTier,
        string? description,
        DateTimeOffset? expiresAt)
    {
        Id = Guid.NewGuid();
        Name = name;
        KeyHash = keyHash;
        KeyPrefix = keyPrefix;
        Scope = scope;
        Status = ApiKeyStatus.Active;
        Permissions = [.. permissions];
        CustomerId = customerId;
        StorefrontId = storefrontId;
        Environment = environment;
        RateLimitTier = rateLimitTier;
        RateLimitRequestsPerMinute = ApiKeyRateLimitPolicy.GetRequestsPerMinute(environment, rateLimitTier);
        Description = description;
        ExpiresAt = expiresAt;
    }

    public string Name { get; private set; }

    public string KeyHash { get; private set; }

    public string KeyPrefix { get; private set; }

    public ApiKeyScope Scope { get; private set; }

    public ApiKeyStatus Status { get; private set; }

    public List<string> Permissions { get; private set; } = [];

    public Guid? CustomerId { get; private set; }

    public Guid? StorefrontId { get; private set; }

    public ApiKeyEnvironment Environment { get; private set; }

    public ApiKeyRateLimitTier RateLimitTier { get; private set; }

    public int RateLimitRequestsPerMinute { get; private set; }

    public string? Description { get; private set; }

    public DateTimeOffset? ExpiresAt { get; private set; }

    public DateTimeOffset? LastUsedAt { get; private set; }

    public DateTimeOffset? LastRotatedAt { get; private set; }

    public int RotationCount { get; private set; }

    public string? PreviousKeyPrefix { get; private set; }

    public IReadOnlyCollection<ApiKeyUsageEntity> UsageRecords => _usageRecords.AsReadOnly();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public static ApiKeyEntity Create(
        string name,
        string keyHash,
        string keyPrefix,
        ApiKeyScope scope,
        IReadOnlyList<string> permissions,
        string? description = null,
        DateTimeOffset? expiresAt = null)
    {
        return Create(
            name,
            keyHash,
            keyPrefix,
            scope,
            permissions,
            customerId: null,
            storefrontId: null,
            ApiKeyEnvironment.Production,
            ApiKeyRateLimitTier.Standard,
            description,
            expiresAt);
    }

    public static ApiKeyEntity Create(
        string name,
        string keyHash,
        string keyPrefix,
        ApiKeyScope scope,
        IReadOnlyList<string> permissions,
        Guid? customerId,
        ApiKeyEnvironment environment,
        ApiKeyRateLimitTier rateLimitTier,
        string? description = null,
        DateTimeOffset? expiresAt = null)
    {
        return Create(
            name,
            keyHash,
            keyPrefix,
            scope,
            permissions,
            customerId,
            storefrontId: null,
            environment,
            rateLimitTier,
            description,
            expiresAt);
    }

    public static ApiKeyEntity Create(
        string name,
        string keyHash,
        string keyPrefix,
        ApiKeyScope scope,
        IReadOnlyList<string> permissions,
        Guid? customerId,
        Guid? storefrontId,
        ApiKeyEnvironment environment,
        ApiKeyRateLimitTier rateLimitTier,
        string? description = null,
        DateTimeOffset? expiresAt = null)
    {
        var entity = new ApiKeyEntity(
            Require(name, nameof(name)),
            Require(keyHash, nameof(keyHash)),
            Require(keyPrefix, nameof(keyPrefix)),
            scope,
            permissions,
            customerId,
            storefrontId,
            environment,
            rateLimitTier,
            description,
            expiresAt);

        entity.AddDomainEvent(new ApiKeyCreatedDomainEvent(entity.Id, entity.Name, scope, storefrontId));

        return entity;
    }

    public void Update(
        string? name,
        string? description,
        IReadOnlyList<string>? permissions,
        DateTimeOffset? expiresAt,
        Guid? customerId = null,
        ApiKeyEnvironment? environment = null,
        ApiKeyRateLimitTier? rateLimitTier = null)
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

        if (customerId.HasValue)
        {
            CustomerId = customerId;
        }

        if (environment.HasValue)
        {
            Environment = environment.Value;
        }

        if (rateLimitTier.HasValue)
        {
            RateLimitTier = rateLimitTier.Value;
        }

        if (environment.HasValue || rateLimitTier.HasValue)
        {
            RateLimitRequestsPerMinute = ApiKeyRateLimitPolicy.GetRequestsPerMinute(Environment, RateLimitTier);
        }
    }

    public void Revoke()
    {
        Status = ApiKeyStatus.Revoked;
        AddDomainEvent(new ApiKeyRevokedDomainEvent(Id, Name));
    }

    public void Regenerate(string newKeyHash, string newKeyPrefix)
    {
        Require(newKeyHash, nameof(newKeyHash));
        Require(newKeyPrefix, nameof(newKeyPrefix));

        PreviousKeyPrefix = KeyPrefix;
        KeyHash = newKeyHash;
        KeyPrefix = newKeyPrefix;
        LastRotatedAt = DateTimeOffset.UtcNow;
        RotationCount++;
        AddDomainEvent(new ApiKeyRegeneratedDomainEvent(Id, Name));
    }

    public void RecordUsage()
    {
        LastUsedAt = DateTimeOffset.UtcNow;
    }

    public bool IsExpired() => ExpiresAt.HasValue && ExpiresAt.Value <= DateTimeOffset.UtcNow;

    public bool IsActive() => Status == ApiKeyStatus.Active && !IsExpired();

    public bool HasPermission(string permission) => Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);

    public void ClearDomainEvents() => _domainEvents.Clear();

    private void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    private static string Require(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{name} must be provided", name);
        }

        return value;
    }
}
