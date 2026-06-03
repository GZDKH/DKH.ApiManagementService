using DKH.Platform.Modularity;

namespace DKH.ApiManagementService.Application.Abstractions;

/// <summary>
///     Supplies module and edition declarations (from <c>module.json</c> / <c>edition.json</c>) to the catalog.
/// </summary>
public interface IModuleManifestSource
{
    Task<IReadOnlyList<PlatformModuleManifest>> GetComponentsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PlatformEditionManifest>> GetEditionsAsync(CancellationToken cancellationToken = default);
}
