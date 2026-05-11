using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Application.Features.ApiKeys.Mappers;
using DKH.ApiManagementService.Domain.Authorization;
using DKH.ApiManagementService.Domain.Entities;
using DKH.Platform.Authorization.ResourceAccess;
using DKH.Platform.Authorization.ResourceAccess.EntityFrameworkCore.QueryFilters;
using DKH.Platform.EntityFrameworkCore;
using DKH.Platform.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Queries.ListApiKeys;

public sealed class ListApiKeysQueryHandler(IAppDbContext dbContext, IPlatformCurrentUser currentUser)
    : IRequestHandler<ListApiKeysQuery, ListApiKeysResult>
{
    public async Task<ListApiKeysResult> Handle(ListApiKeysQuery request, CancellationToken cancellationToken)
    {
        var ef = (DbContext)dbContext;
        var query = dbContext.ApiKeys
            .AsNoTracking()
            .ApplySoftDeleteFilter(request.SoftDeleteFilter)
            .ApplyResourceAccessFilter<ApiKeyEntity, ApiManagementAccessGrantEntity, Guid>(
                ef,
                currentUser,
                resourceType: "api_key",
                permission: ResourceAccessPermissions.Read,
                resourceIdSelector: x => x.Id);

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
