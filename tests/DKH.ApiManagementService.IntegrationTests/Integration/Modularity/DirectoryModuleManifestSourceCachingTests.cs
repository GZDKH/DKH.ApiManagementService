using DKH.ApiManagementService.Infrastructure.Modularity;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace DKH.ApiManagementService.IntegrationTests.Integration.Modularity;

/// <summary>
///     Guards the load-once caching contract of <see cref="DirectoryModuleManifestSource" />: manifests are
///     content-copied and immutable for the process lifetime, so the directory is scanned and parsed exactly
///     once per kind. Proven behaviourally — a manifest written AFTER the first read must NOT appear in a
///     later read (a re-scanning source would pick it up).
/// </summary>
[Trait("Category", "Integration")]
public sealed class DirectoryModuleManifestSourceCachingTests : IDisposable
{
    private readonly string _directory =
        Path.Combine(Path.GetTempPath(), $"dkh-manifests-{Guid.NewGuid():N}");

    public DirectoryModuleManifestSourceCachingTests() => Directory.CreateDirectory(_directory);

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [Fact]
    public async Task GetComponents_CachesFirstScan_IgnoringManifestsAddedLaterAsync()
    {
        WriteServiceManifest("dkh.first");
        var source = CreateSource();

        var first = await source.GetComponentsAsync();
        first.Select(component => component.Id).Should().BeEquivalentTo("dkh.first");

        // A re-scanning source would surface this; a cached one will not.
        WriteServiceManifest("dkh.second");
        var second = await source.GetComponentsAsync();

        second.Select(component => component.Id).Should().BeEquivalentTo("dkh.first");
    }

    private void WriteServiceManifest(string id)
    {
        var dir = Directory.CreateDirectory(Path.Combine(_directory, id));
        File.WriteAllText(
            Path.Combine(dir.FullName, "module.json"),
            $$"""
            {
              "id": "{{id}}",
              "kind": "Service",
              "name": { "en": "{{id}}" },
              "version": "1.0.0"
            }
            """);
    }

    private DirectoryModuleManifestSource CreateSource()
        => new(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Modularity:ManifestsDirectory"] = _directory,
            })
            .Build());
}
