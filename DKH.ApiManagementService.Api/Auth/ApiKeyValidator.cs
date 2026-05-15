using System.Security.Claims;
using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Domain.Entities;
using DKH.ApiManagementService.Domain.ValueObjects;
using DKH.Platform.ApiKeyAuth;
using Microsoft.Extensions.Logging;

namespace DKH.ApiManagementService.Api.Auth;

/// <summary>
///     Validates an inbound API key against the <see cref="ApiKeyEntity" /> store.
/// </summary>
/// <remarks>
///     Implements <see cref="IPlatformApiKeyValidator" /> for <c>AddPlatformApiKeyAuth</c>.
///     The presented raw key is hashed with the same SHA-256 (lowercase hex) algorithm used
///     when keys are issued (see <see cref="ApiKeyHash.FromRawKey" />), then looked up by hash.
///     A key authenticates only when active (not revoked, not expired). Permissions are
///     projected onto claims so downstream authorization can read them through the standard
///     <see cref="ClaimsPrincipal" /> surface and <see cref="PlatformApiKeyContext" /> in
///     <c>HttpContext.Items</c>.
/// </remarks>
/// <remarks>
///     The Platform helper registers this validator as a singleton, so a per-call scope is
///     created via <see cref="IServiceScopeFactory" /> to resolve the scoped
///     <see cref="IApiKeyRepository" /> (backed by a scoped <c>AppDbContext</c>).
/// </remarks>
public sealed class ApiKeyValidator(
    IServiceScopeFactory scopeFactory,
    ILogger<ApiKeyValidator> logger) : IPlatformApiKeyValidator
{
    /// <summary>Claim type carrying granted permissions on the authenticated principal.</summary>
    public const string PermissionClaimType = "permission";

    /// <summary>Claim type carrying the API key id (resource identifier of the <see cref="ApiKeyEntity" />).</summary>
    public const string ApiKeyIdClaimType = "api_key_id";

    /// <summary>Claim type carrying the API key scope (mcp, webhook, partner, storefront, internal).</summary>
    public const string ApiKeyScopeClaimType = "api_key_scope";

    public async Task<PlatformApiKeyValidationResult> ValidateAsync(
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return PlatformApiKeyValidationResult.Invalid;
        }

        var keyHash = ApiKeyHash.FromRawKey(apiKey).Value;

        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IApiKeyRepository>();

        var entity = await repository.GetByKeyHashAsync(keyHash, cancellationToken).ConfigureAwait(false);

        if (entity is null)
        {
            logger.LogWarning("API key authentication failed: key not found");
            return PlatformApiKeyValidationResult.Invalid;
        }

        if (!entity.IsActive())
        {
            logger.LogWarning(
                "API key authentication failed: key {ApiKeyId} is not active (expired={Expired})",
                entity.Id,
                entity.IsExpired());
            return PlatformApiKeyValidationResult.Invalid;
        }

        entity.RecordUsage();
        await repository.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, entity.Name),
            new(ClaimTypes.NameIdentifier, entity.Id.ToString()),
            new(ApiKeyIdClaimType, entity.Id.ToString()),
            new(ApiKeyScopeClaimType, entity.Scope.ToString()),
        };

        claims.AddRange(entity.Permissions.Select(permission => new Claim(PermissionClaimType, permission)));

        return new PlatformApiKeyValidationResult(
            IsValid: true,
            ClientName: entity.Name,
            Claims: claims);
    }
}
