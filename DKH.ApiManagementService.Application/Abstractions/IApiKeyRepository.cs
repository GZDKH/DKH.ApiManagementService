using DKH.ApiManagementService.Domain.Entities;

namespace DKH.ApiManagementService.Application.Abstractions;

public interface IApiKeyRepository
{
    Task<ApiKeyEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiKeyEntity?> GetByKeyHashAsync(string keyHash, CancellationToken cancellationToken = default);

    Task AddAsync(ApiKeyEntity apiKey, CancellationToken cancellationToken = default);

    Task UpdateAsync(ApiKeyEntity apiKey, CancellationToken cancellationToken = default);

    Task DeleteAsync(ApiKeyEntity apiKey, CancellationToken cancellationToken = default);
}
