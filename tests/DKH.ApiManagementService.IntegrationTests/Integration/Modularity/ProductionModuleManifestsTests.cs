using DKH.ApiManagementService.Infrastructure.Modularity;
using DKH.Platform.Modularity;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace DKH.ApiManagementService.IntegrationTests.Integration.Modularity;

/// <summary>
///     Guards the PRODUCTION module/edition manifests shipped by DKH.ApiManagementService.Api
///     (<c>Modularity/manifests</c>, content-copied into the test output via the project reference).
///     They must parse, resolve into a coherent catalog, and — critically — core service manifests must
///     carry NO <c>requiresEntitlement</c>. That, combined with GetModuleState defaulting a manifest-present
///     module to Enabled, is what keeps the Admin nav sections (navigation.ts maps them by capabilityModuleId)
///     resolving to <c>visible</c> rather than hidden/locked (the 2026-06-05 activation regression).
/// </summary>
[Trait("Category", "Integration")]
public sealed class ProductionModuleManifestsTests
{
    private static readonly string ManifestsDirectory =
        Path.Combine(AppContext.BaseDirectory, "Modularity", "manifests");

    // Admin nav sections that gate by capabilityModuleId (ui/.../shared/config/navigation.ts). Shipping a
    // manifest for any of these means it must stay visible — i.e. NO requiresEntitlement.
    private static readonly string[] NavMappedServiceIds =
    [
        "dkh.product-catalog", "dkh.payments", "dkh.logistics", "dkh.customs",
    ];

    private static readonly string[] ExpectedPluginIds =
    [
        "dkh.ai.claude", "dkh.payments.stripe", "dkh.payments.telegram",
    ];

    // The curated production service catalog (Phase 6 sweep — one component per deployed bounded context).
    // Exact-match guard: adding/removing a service manifest is a deliberate, reviewed change to the catalog.
    private static readonly string[] ExpectedServiceIds =
    [
        "dkh.api-management", "dkh.assistant", "dkh.broadcast", "dkh.cart", "dkh.counterparty",
        "dkh.customers", "dkh.customs", "dkh.delivery", "dkh.engagement", "dkh.inventory", "dkh.logistics",
        "dkh.media", "dkh.notifications", "dkh.orders", "dkh.payments", "dkh.print",
        "dkh.procurement", "dkh.product-catalog", "dkh.product-request", "dkh.reference", "dkh.reviews",
        "dkh.search", "dkh.staff", "dkh.storefront", "dkh.subscription", "dkh.telegram-bot",
        "dkh.telegram-client", "dkh.warehouse",
    ];

    private static DirectoryModuleManifestSource CreateSource()
        => new(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Modularity:ManifestsDirectory"] = ManifestsDirectory,
            })
            .Build());

    [Fact]
    public async Task ProductionManifests_ParseWithExpectedServicesAndPluginsAsync()
    {
        var components = await CreateSource().GetComponentsAsync();
        var ids = components.Select(component => component.Id).ToList();

        ids.Should().Contain(NavMappedServiceIds);
        components.Where(component => component.Kind == PlatformModuleKind.Plugin).Select(component => component.Id)
            .Should().BeEquivalentTo(ExpectedPluginIds);
        components.Should().OnlyContain(component =>
            !string.IsNullOrWhiteSpace(component.Id)
            && !string.IsNullOrWhiteSpace(component.Version)
            && component.Name.Count > 0);
    }

    [Fact]
    public async Task ProductionManifests_ShipTheCuratedServiceCatalogAsync()
    {
        var components = await CreateSource().GetComponentsAsync();

        components
            .Where(component => component.Kind == PlatformModuleKind.Service)
            .Select(component => component.Id)
            .Should().BeEquivalentTo(ExpectedServiceIds);
    }

    [Fact]
    public async Task AppSettings_EnablesEveryServiceManifest_AndLeavesPluginsOptInAsync()
    {
        var components = await CreateSource().GetComponentsAsync();
        var serviceIds = components
            .Where(component => component.Kind == PlatformModuleKind.Service)
            .Select(component => component.Id);
        var pluginIds = components
            .Where(component => component.Kind == PlatformModuleKind.Plugin)
            .Select(component => component.Id);
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), optional: false)
            .Build();

        var enabledModuleIds = configuration
            .GetSection(PlatformConfigurationModuleEntitlementProvider.ConfigurationKey)
            .Get<string[]>() ?? [];

        enabledModuleIds.Should().BeEquivalentTo(serviceIds);
        enabledModuleIds.Should().NotContain(pluginIds);
    }

    [Fact]
    public async Task CoreServiceManifests_HaveNoEntitlement_SoTheyResolveVisibleAsync()
    {
        var components = await CreateSource().GetComponentsAsync();

        // Every shipped SERVICE is core platform → free → entitled → visible. Only plugins/editions gate.
        components
            .Where(component => component.Kind == PlatformModuleKind.Service)
            .Should().OnlyContain(component => string.IsNullOrEmpty(component.RequiresEntitlement));

        // Plugins are optional add-ons → they DO require entitlement (locked until bought).
        components
            .Where(component => component.Kind == PlatformModuleKind.Plugin)
            .Should().OnlyContain(component => !string.IsNullOrEmpty(component.RequiresEntitlement));
    }

    [Fact]
    public async Task ProductionManifests_ResolveIntoCoherentDependencyGraphAsync()
    {
        var source = CreateSource();
        var components = await source.GetComponentsAsync();
        var editions = await source.GetEditionsAsync();

        var result = PlatformModuleDependencyResolver.Resolve(components, editions);

        result.IsSuccessful.Should().BeTrue(
            "production manifests must resolve cleanly, but found: {0}",
            string.Join("; ", result.Problems));
        result.Order.Should().HaveCount(components.Count);
    }

    [Fact]
    public async Task ProductionManifests_EditionsReferenceKnownComponentsAsync()
    {
        var source = CreateSource();
        var editions = await source.GetEditionsAsync();
        var componentIds = (await source.GetComponentsAsync())
            .Select(component => component.Id)
            .ToHashSet(StringComparer.Ordinal);

        editions.Select(edition => edition.Id).Should().BeEquivalentTo(["commerce-suite", "logistics-suite"]);
        editions.SelectMany(edition => edition.Components)
            .Should().OnlyContain(id => componentIds.Contains(id));
    }
}
