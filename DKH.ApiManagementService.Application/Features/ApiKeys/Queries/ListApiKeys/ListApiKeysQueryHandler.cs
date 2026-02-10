using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Application.Features.ApiKeys.Mappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Queries.ListApiKeys;

public sealed class ListApiKeysQueryHandler(IAppDbContext dbContext) : IRequestHandler<ListApiKeysQuery, ListApiKeysResult>
{
    public async Task<ListApiKeysResult> Handle(ListApiKeysQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.ApiKeys.AsNoTracking().AsQueryable();

        if (request.ScopeFilter.HasValue)
        {
            query = query.Where(x => x.Scope == request.ScopeFilter.Value);
        }

        if (request.StatusFilter.HasValue)
        {
            query = query.Where(x => x.Status == request.StatusFilter.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var entities = await query
            .OrderByDescending(x => x.CreationTime)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var apiKeys = entities.Select(e => e.ToProto()).ToList();

        return new ListApiKeysResult(apiKeys, totalCount);
    }
}
