using DKH.ApiManagementService.Domain.Enums;
using DKH.Platform.Domain.Entities.Auditing;

namespace DKH.ApiManagementService.Domain.Entities;

/// <summary>
///     A plugin component that a host service has reported into the catalog at runtime.
///     Side-loaded plugins live in other services' processes and have no local <c>module.json</c>,
///     so host services call <c>ReportComponent</c> to surface them in the module catalog.
/// </summary>
public sealed class ReportedModuleComponentEntity : FullAuditedEntityWithKey<Guid>
{
    private ReportedModuleComponentEntity()
    {
        ModuleId = string.Empty;
        Kind = string.Empty;
        Version = string.Empty;
        Name = new Dictionary<string, string>(StringComparer.Ordinal);
        Provides = [];
        Requires = [];
    }

    private ReportedModuleComponentEntity(
        string moduleId,
        string kind,
        Dictionary<string, string> name,
        string version,
        Dictionary<string, string>? description,
        string? category,
        List<ReportedCapability> provides,
        List<ReportedDependency> requires,
        string? requiresEntitlement,
        ModuleLifecycleState state)
    {
        Id = Guid.NewGuid();
        ModuleId = moduleId;
        Kind = kind;
        Name = name;
        Version = version;
        Description = description;
        Category = category;
        Provides = provides;
        Requires = requires;
        RequiresEntitlement = requiresEntitlement;
        State = state;
    }

    /// <summary>Stable component identifier (e.g. "payments.stripe").</summary>
    public string ModuleId { get; private set; }

    /// <summary>Execution kind: "Plugin" or "Service".</summary>
    public string Kind { get; private set; }

    /// <summary>Localized display name keyed by culture (e.g. "en", "ru").</summary>
    public Dictionary<string, string> Name { get; private set; }

    /// <summary>Semantic version string reported by the plugin host.</summary>
    public string Version { get; private set; }

    /// <summary>Optional localized description keyed by culture.</summary>
    public Dictionary<string, string>? Description { get; private set; }

    /// <summary>Functional category (e.g. "Commerce/Payments"); null when not provided.</summary>
    public string? Category { get; private set; }

    /// <summary>Capabilities this component advertises.</summary>
    public List<ReportedCapability> Provides { get; private set; }

    /// <summary>Capabilities this component depends on.</summary>
    public List<ReportedDependency> Requires { get; private set; }

    /// <summary>Entitlement key required to activate this component; null when none required.</summary>
    public string? RequiresEntitlement { get; private set; }

    /// <summary>Lifecycle state as last reported by the host service.</summary>
    public ModuleLifecycleState State { get; private set; }

    public static ReportedModuleComponentEntity Create(
        string moduleId,
        string kind,
        Dictionary<string, string> name,
        string version,
        Dictionary<string, string>? description,
        string? category,
        List<ReportedCapability> provides,
        List<ReportedDependency> requires,
        string? requiresEntitlement,
        ModuleLifecycleState state)
        => new(
            Require(moduleId, nameof(moduleId)),
            Require(kind, nameof(kind)),
            name,
            Require(version, nameof(version)),
            description,
            string.IsNullOrWhiteSpace(category) ? null : category,
            provides,
            requires,
            string.IsNullOrWhiteSpace(requiresEntitlement) ? null : requiresEntitlement,
            state);

    public void Update(
        string kind,
        Dictionary<string, string> name,
        string version,
        Dictionary<string, string>? description,
        string? category,
        List<ReportedCapability> provides,
        List<ReportedDependency> requires,
        string? requiresEntitlement,
        ModuleLifecycleState state)
    {
        Kind = Require(kind, nameof(kind));
        Name = name;
        Version = Require(version, nameof(version));
        Description = description;
        Category = string.IsNullOrWhiteSpace(category) ? null : category;
        Provides = provides;
        Requires = requires;
        RequiresEntitlement = string.IsNullOrWhiteSpace(requiresEntitlement) ? null : requiresEntitlement;
        State = state;
    }

    private static string Require(string value, string name)
        => string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException($"{name} must be provided", name)
            : value;
}

/// <summary>A capability this component provides.</summary>
public sealed record ReportedCapability(string Id, string Version);

/// <summary>A capability this component depends on, with an optional version range.</summary>
public sealed record ReportedDependency(string CapabilityId, string VersionRange);
