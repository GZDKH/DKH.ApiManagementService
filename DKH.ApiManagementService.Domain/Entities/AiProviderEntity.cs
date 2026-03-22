using DKH.ApiManagementService.Domain.Entities.Events;
using DKH.ApiManagementService.Domain.Enums;
using DKH.Platform.Domain.Entities.Auditing;
using DKH.Platform.Domain.Events;

namespace DKH.ApiManagementService.Domain.Entities;

public sealed class AiProviderEntity : FullAuditedEntityWithKey<Guid>, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];

    private AiProviderEntity()
    {
        Name = string.Empty;
    }

    private AiProviderEntity(
        string name,
        AiProviderType providerType,
        AiProviderStatus status,
        string? displayName,
        string? baseUrl,
        List<string> models,
        string? apiKeyReference,
        int? rateLimitPerMinute,
        int? dailyQuota)
    {
        Id = Guid.NewGuid();
        Name = name;
        ProviderType = providerType;
        Status = status;
        DisplayName = displayName;
        BaseUrl = baseUrl;
        Models = models;
        ApiKeyReference = apiKeyReference;
        RateLimitPerMinute = rateLimitPerMinute;
        DailyQuota = dailyQuota;
    }

    public string Name { get; private set; }

    public AiProviderType ProviderType { get; private set; }

    public AiProviderStatus Status { get; private set; }

    public string? DisplayName { get; private set; }

    public string? BaseUrl { get; private set; }

    public List<string> Models { get; private set; } = [];

    public string? ApiKeyReference { get; private set; }

    public int? RateLimitPerMinute { get; private set; }

    public int? DailyQuota { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public static AiProviderEntity Create(
        string name,
        AiProviderType providerType,
        string? displayName = null,
        string? baseUrl = null,
        List<string>? models = null,
        string? apiKeyReference = null,
        int? rateLimitPerMinute = null,
        int? dailyQuota = null)
    {
        var entity = new AiProviderEntity(
            Require(name, nameof(name)),
            providerType,
            AiProviderStatus.Active,
            displayName,
            baseUrl,
            models ?? [],
            apiKeyReference,
            rateLimitPerMinute,
            dailyQuota);

        entity.AddDomainEvent(new AiProviderCreatedDomainEvent(entity.Id, entity.Name, providerType));

        return entity;
    }

    public void Update(
        string? name,
        string? displayName,
        string? baseUrl,
        List<string>? models,
        string? apiKeyReference,
        int? rateLimitPerMinute,
        int? dailyQuota)
    {
        if (name is not null)
        {
            Name = name;
        }

        if (displayName is not null)
        {
            DisplayName = displayName;
        }

        if (baseUrl is not null)
        {
            BaseUrl = baseUrl;
        }

        if (models is not null)
        {
            Models = [.. models];
        }

        if (apiKeyReference is not null)
        {
            ApiKeyReference = apiKeyReference;
        }

        if (rateLimitPerMinute is not null)
        {
            RateLimitPerMinute = rateLimitPerMinute;
        }

        if (dailyQuota is not null)
        {
            DailyQuota = dailyQuota;
        }

        AddDomainEvent(new AiProviderUpdatedDomainEvent(Id, Name));
    }

    public void Activate()
    {
        Status = AiProviderStatus.Active;
    }

    public void Deactivate()
    {
        Status = AiProviderStatus.Inactive;
    }

    public void MarkError()
    {
        Status = AiProviderStatus.Error;
    }

    public bool IsActive() => Status == AiProviderStatus.Active;

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
