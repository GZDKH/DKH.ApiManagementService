using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DKH.ApiManagementService.Infrastructure.Persistence.Repositories;

public class ApiKeyRepository(AppDbContext dbContext) : IApiKeyRepository
{
    public async Task<ApiKeyEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.ApiKeys.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<ApiKeyEntity?> GetByKeyHashAsync(string keyHash, CancellationToken cancellationToken = default)
    {
        return await dbContext.ApiKeys.FirstOrDefaultAsync(x => x.KeyHash == keyHash, cancellationToken);
    }

    public async Task AddAsync(ApiKeyEntity apiKey, CancellationToken cancellationToken = default)
    {
        await dbContext.ApiKeys.AddAsync(apiKey, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ApiKeyEntity apiKey, CancellationToken cancellationToken = default)
    {
        dbContext.ApiKeys.Update(apiKey);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ApiKeyEntity apiKey, CancellationToken cancellationToken = default)
    {
        dbContext.ApiKeys.Remove(apiKey);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
