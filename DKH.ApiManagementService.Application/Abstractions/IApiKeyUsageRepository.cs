using DKH.ApiManagementService.Domain.Entities;

namespace DKH.ApiManagementService.Application.Abstractions;

public interface IApiKeyUsageRepository
{
    Task AddAsync(ApiKeyUsageEntity usage, CancellationToken cancellationToken = default);
}
