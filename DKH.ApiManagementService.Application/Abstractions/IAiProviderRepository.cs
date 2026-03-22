using DKH.ApiManagementService.Domain.Entities;
using DKH.ApiManagementService.Domain.Enums;

namespace DKH.ApiManagementService.Application.Abstractions;

public interface IAiProviderRepository
{
    Task<AiProviderEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AiProviderEntity?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<AiProviderEntity> Items, int TotalCount)> ListAsync(
        int page,
        int pageSize,
        AiProviderType? typeFilter = null,
        AiProviderStatus? statusFilter = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AiProviderEntity>> GetByTypeAsync(AiProviderType providerType, CancellationToken cancellationToken = default);

    Task AddAsync(AiProviderEntity entity, CancellationToken cancellationToken = default);

    Task UpdateAsync(AiProviderEntity entity, CancellationToken cancellationToken = default);

    Task DeleteAsync(AiProviderEntity entity, CancellationToken cancellationToken = default);
}
