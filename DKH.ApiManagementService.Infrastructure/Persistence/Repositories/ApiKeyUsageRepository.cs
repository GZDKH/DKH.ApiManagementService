using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Domain.Entities;

namespace DKH.ApiManagementService.Infrastructure.Persistence.Repositories;

public class ApiKeyUsageRepository(AppDbContext dbContext) : IApiKeyUsageRepository
{
    public async Task AddAsync(ApiKeyUsageEntity usage, CancellationToken cancellationToken = default)
    {
        await dbContext.ApiKeyUsageRecords.AddAsync(usage, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
