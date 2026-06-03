using DKH.ApiManagementService.Domain.Enums;
using DKH.Platform.Domain.Entities.Auditing;
using DKH.Platform.Domain.Events;

namespace DKH.ApiManagementService.Domain.Entities;

/// <summary>
///     An entitlement record: whether a module or edition is granted for a given scope.
/// </summary>
public sealed class ModuleEntitlementEntity : FullAuditedEntityWithKey<Guid>, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];

    private ModuleEntitlementEntity()
    {
        ModuleId = string.Empty;
    }

    private ModuleEntitlementEntity(ModuleEntitlementScopeKind scopeKind, string? scopeValue, string moduleId, bool granted)
    {
        Id = Guid.NewGuid();
        ScopeKind = scopeKind;
        ScopeValue = scopeValue;
        ModuleId = moduleId;
        Granted = granted;
    }

    public ModuleEntitlementScopeKind ScopeKind { get; private set; }

    public string? ScopeValue { get; private set; }

    public string ModuleId { get; private set; }

    public bool Granted { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public static ModuleEntitlementEntity Create(
        ModuleEntitlementScopeKind scopeKind,
        string? scopeValue,
        string moduleId,
        bool granted = true)
        => new(scopeKind, scopeValue, Require(moduleId, nameof(moduleId)), granted);

    public void Grant() => Granted = true;

    public void Revoke() => Granted = false;

    public void ClearDomainEvents() => _domainEvents.Clear();

    private static string Require(string value, string name)
        => string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException($"{name} must be provided", name)
            : value;
}
