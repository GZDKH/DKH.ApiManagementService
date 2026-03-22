using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Domain.Entities;
using DKH.ApiManagementService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DKH.ApiManagementService.Infrastructure.Persistence.Repositories;

public class AiProviderRepository(AppDbContext dbContext) : IAiProviderRepository
{
    public async Task<AiProviderEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.AiProviders
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<AiProviderEntity?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await dbContext.AiProviders
            .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
    }

    public async Task<(IReadOnlyList<AiProviderEntity> Items, int TotalCount)> ListAsync(
        int page,
        int pageSize,
        AiProviderType? typeFilter = null,
        AiProviderStatus? statusFilter = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.AiProviders.AsNoTracking().AsQueryable();

        if (typeFilter.HasValue)
        {
            query = query.Where(x => x.ProviderType == typeFilter.Value);
        }

        if (statusFilter.HasValue)
        {
            query = query.Where(x => x.Status == statusFilter.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<AiProviderEntity>> GetByTypeAsync(AiProviderType providerType, CancellationToken cancellationToken = default)
    {
        return await dbContext.AiProviders
            .AsNoTracking()
            .Where(x => x.ProviderType == providerType)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(AiProviderEntity entity, CancellationToken cancellationToken = default)
    {
        await dbContext.AiProviders.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(AiProviderEntity entity, CancellationToken cancellationToken = default)
    {
        dbContext.AiProviders.Update(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(AiProviderEntity entity, CancellationToken cancellationToken = default)
    {
        dbContext.AiProviders.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
