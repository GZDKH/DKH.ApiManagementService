using DKH.ApiManagementService.Domain.Enums;
using DKH.Platform.Domain.Entities.Auditing;
using DKH.Platform.Domain.Events;

namespace DKH.ApiManagementService.Domain.Entities;

/// <summary>
///     The deployment-level lifecycle state of a module component in the control-plane.
/// </summary>
public sealed class ModuleStateEntity : FullAuditedEntityWithKey<Guid>, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];

    private ModuleStateEntity()
    {
        ModuleId = string.Empty;
        Version = string.Empty;
    }

    private ModuleStateEntity(string moduleId, ModuleLifecycleState state, string version)
    {
        Id = Guid.NewGuid();
        ModuleId = moduleId;
        State = state;
        Version = version;
    }

    public string ModuleId { get; private set; }

    public ModuleLifecycleState State { get; private set; }

    public string Version { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public static ModuleStateEntity Create(
        string moduleId,
        string version,
        ModuleLifecycleState state = ModuleLifecycleState.Discovered)
        => new(Require(moduleId, nameof(moduleId)), state, Require(version, nameof(version)));

    public void Install(string version)
    {
        Version = Require(version, nameof(version));
        State = ModuleLifecycleState.Installed;
    }

    public void Enable() => State = ModuleLifecycleState.Enabled;

    public void Disable() => State = ModuleLifecycleState.Disabled;

    public void MarkFailed() => State = ModuleLifecycleState.Failed;

    public void ClearDomainEvents() => _domainEvents.Clear();

    private static string Require(string value, string name)
        => string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException($"{name} must be provided", name)
            : value;
}
