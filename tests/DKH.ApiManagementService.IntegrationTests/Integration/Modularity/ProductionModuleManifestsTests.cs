using DKH.ApiManagementService.Infrastructure.Modularity;
using DKH.Platform.Modularity;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace DKH.ApiManagementService.IntegrationTests.Integration.Modularity;

/// <summary>
///     Guards the PRODUCTION module/edition manifests shipped by DKH.ApiManagementService.Api
///     (<c>Modularity/manifests</c>, content-copied into the test output via the project reference).
///     They must parse and resolve into a coherent catalog so the live <c>ListComponents</c> / capabilities
///     surface never serves a broken catalog after activation.
/// </summary>
[Trait("Category", "Integration")]
public sealed class ProductionModuleManifestsTests
{
    private static readonly string ManifestsDirectory =
        Path.Combine(AppContext.BaseDirectory, "Modularity", "manifests");

    private static readonly string[] ExpectedServiceIds =
    [
        "dkh.cart", "dkh.customers", "dkh.inventory", "dkh.logistics", "dkh.notifications",
        "dkh.orders", "dkh.payments", "dkh.product-catalog", "dkh.reference", "dkh.reviews", "dkh.storefront",
    ];

    private static readonly string[] ExpectedPluginIds =
    [
        "dkh.ai.claude", "dkh.payments.stripe", "dkh.payments.telegram",
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

        ids.Should().Contain(ExpectedServiceIds);
        components.Where(component => component.Kind == PlatformModuleKind.Plugin).Select(component => component.Id)
            .Should().BeEquivalentTo(ExpectedPluginIds);
        components.Should().OnlyContain(component =>
            !string.IsNullOrWhiteSpace(component.Id)
            && !string.IsNullOrWhiteSpace(component.Version)
            && component.Name.Count > 0);
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
