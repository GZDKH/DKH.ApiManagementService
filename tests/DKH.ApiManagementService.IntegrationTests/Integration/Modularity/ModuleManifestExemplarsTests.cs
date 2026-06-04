using DKH.ApiManagementService.Infrastructure.Modularity;
using DKH.Platform.Modularity;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace DKH.ApiManagementService.IntegrationTests.Integration.Modularity;

/// <summary>
///     Validates the canonical module/edition exemplars (Modularity/Exemplars) end-to-end against the real
///     ingestion path: <see cref="DirectoryModuleManifestSource" /> parsing plus
///     <see cref="PlatformModuleDependencyResolver" /> graph resolution.
/// </summary>
[Trait("Category", "Integration")]
public sealed class ModuleManifestExemplarsTests
{
    private static readonly string ExemplarsDirectory =
        Path.Combine(AppContext.BaseDirectory, "Modularity", "Exemplars");

    private static DirectoryModuleManifestSource CreateSource()
        => new(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Modularity:ManifestsDirectory"] = ExemplarsDirectory,
            })
            .Build());

    [Fact]
    public async Task Exemplars_ParseIntoComponentsWithExpectedKindsAsync()
    {
        var components = await CreateSource().GetComponentsAsync();

        components.Should().HaveCount(6);
        components.Count(component => component.Kind == PlatformModuleKind.Service).Should().Be(3);
        components.Count(component => component.Kind == PlatformModuleKind.Plugin).Should().Be(3);
        components.Should().OnlyContain(component =>
            !string.IsNullOrWhiteSpace(component.Id)
            && !string.IsNullOrWhiteSpace(component.Version)
            && component.Name.Count > 0);
    }

    [Fact]
    public async Task Exemplars_ResolveIntoCoherentDependencyGraphAsync()
    {
        var source = CreateSource();
        var components = await source.GetComponentsAsync();
        var editions = await source.GetEditionsAsync();

        var result = PlatformModuleDependencyResolver.Resolve(components, editions);

        result.IsSuccessful.Should().BeTrue(
            "exemplar manifests must resolve cleanly, but found: {0}",
            string.Join("; ", result.Problems));
        result.Order.Should().HaveCount(components.Count);

        var order = result.Order.ToList();
        order.IndexOf("dkh.product-catalog").Should().BeLessThan(order.IndexOf("dkh.payments"));
        order.IndexOf("dkh.payments").Should().BeLessThan(order.IndexOf("dkh.payments.stripe"));
        order.IndexOf("dkh.product-catalog").Should().BeLessThan(order.IndexOf("dkh.logistics"));
    }

    [Fact]
    public async Task Exemplars_DeclareEditionsReferencingKnownComponentsAsync()
    {
        var source = CreateSource();
        var editions = await source.GetEditionsAsync();
        var componentIds = (await source.GetComponentsAsync())
            .Select(component => component.Id)
            .ToHashSet(StringComparer.Ordinal);

        editions.Should().HaveCount(2);
        editions.SelectMany(edition => edition.Components)
            .Should().OnlyContain(id => componentIds.Contains(id));
    }
}
