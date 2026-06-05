using System.Text.Json;
using DKH.ApiManagementService.Application.Abstractions;
using DKH.Platform.Modularity;
using Microsoft.Extensions.Configuration;

namespace DKH.ApiManagementService.Infrastructure.Modularity;

/// <summary>
///     Reads module and edition declarations from a configured directory (<c>Modularity:ManifestsDirectory</c>),
///     scanning recursively for <c>module.json</c> / <c>edition.json</c>. The deployment is responsible for
///     collecting each component's manifest into that directory. Returns empty when unconfigured or missing.
///     Manifests are content-copied into the app output and immutable for the process lifetime, so each kind is
///     scanned and parsed once (lazily, thread-safe) and the result cached — this source is a singleton hit on
///     every catalog / dependency-graph / state request, and the catalog grows over time.
/// </summary>
public sealed class DirectoryModuleManifestSource : IModuleManifestSource
{
    private const string DirectoryKey = "Modularity:ManifestsDirectory";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly string? _directory;
    private readonly Lazy<Task<IReadOnlyList<PlatformModuleManifest>>> _components;
    private readonly Lazy<Task<IReadOnlyList<PlatformEditionManifest>>> _editions;

    public DirectoryModuleManifestSource(IConfiguration configuration)
    {
        _directory = ResolveDirectory(configuration[DirectoryKey]);
        _components = new Lazy<Task<IReadOnlyList<PlatformModuleManifest>>>(
            () => LoadAsync<PlatformModuleManifest>("module.json"), LazyThreadSafetyMode.ExecutionAndPublication);
        _editions = new Lazy<Task<IReadOnlyList<PlatformEditionManifest>>>(
            () => LoadAsync<PlatformEditionManifest>("edition.json"), LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public Task<IReadOnlyList<PlatformModuleManifest>> GetComponentsAsync(CancellationToken cancellationToken = default)
        => _components.Value;

    public Task<IReadOnlyList<PlatformEditionManifest>> GetEditionsAsync(CancellationToken cancellationToken = default)
        => _editions.Value;

    private async Task<IReadOnlyList<T>> LoadAsync<T>(string fileName)
    {
        if (string.IsNullOrWhiteSpace(_directory) || !Directory.Exists(_directory))
        {
            return [];
        }

        var manifests = new List<T>();
        foreach (var file in Directory.EnumerateFiles(_directory, fileName, SearchOption.AllDirectories))
        {
            await using var stream = File.OpenRead(file);
            var manifest = await JsonSerializer.DeserializeAsync<T>(stream, SerializerOptions);
            if (manifest is not null)
            {
                manifests.Add(manifest);
            }
        }

        return manifests;
    }

    /// <summary>
    ///     Resolves the configured manifests directory. Absolute paths are used as-is; a relative path is
    ///     resolved against <see cref="AppContext.BaseDirectory" /> (the app output directory where deployed
    ///     manifests are content-copied), so the same relative value works in-container and in local dev
    ///     regardless of the process working directory.
    /// </summary>
    private static string? ResolveDirectory(string? configured)
        => string.IsNullOrWhiteSpace(configured) || Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(AppContext.BaseDirectory, configured);
}
