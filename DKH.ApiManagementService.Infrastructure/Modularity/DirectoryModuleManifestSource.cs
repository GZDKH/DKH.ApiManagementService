using System.Text.Json;
using DKH.ApiManagementService.Application.Abstractions;
using DKH.Platform.Modularity;
using Microsoft.Extensions.Configuration;

namespace DKH.ApiManagementService.Infrastructure.Modularity;

/// <summary>
///     Reads module and edition declarations from a configured directory (<c>Modularity:ManifestsDirectory</c>),
///     scanning recursively for <c>module.json</c> / <c>edition.json</c>. The deployment is responsible for
///     collecting each component's manifest into that directory. Returns empty when unconfigured or missing.
/// </summary>
public sealed class DirectoryModuleManifestSource(IConfiguration configuration) : IModuleManifestSource
{
    private const string DirectoryKey = "Modularity:ManifestsDirectory";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly string? _directory = configuration[DirectoryKey];

    public Task<IReadOnlyList<PlatformModuleManifest>> GetComponentsAsync(CancellationToken cancellationToken = default)
        => LoadAsync<PlatformModuleManifest>("module.json", cancellationToken);

    public Task<IReadOnlyList<PlatformEditionManifest>> GetEditionsAsync(CancellationToken cancellationToken = default)
        => LoadAsync<PlatformEditionManifest>("edition.json", cancellationToken);

    private async Task<IReadOnlyList<T>> LoadAsync<T>(string fileName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_directory) || !Directory.Exists(_directory))
        {
            return [];
        }

        var manifests = new List<T>();
        foreach (var file in Directory.EnumerateFiles(_directory, fileName, SearchOption.AllDirectories))
        {
            await using var stream = File.OpenRead(file);
            var manifest = await JsonSerializer.DeserializeAsync<T>(stream, SerializerOptions, cancellationToken);
            if (manifest is not null)
            {
                manifests.Add(manifest);
            }
        }

        return manifests;
    }
}
