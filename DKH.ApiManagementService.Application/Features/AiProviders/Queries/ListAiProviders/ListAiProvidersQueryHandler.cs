using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Application.Features.AiProviders.Mappers;
using DKH.ApiManagementService.Domain.Authorization;
using DKH.ApiManagementService.Domain.Entities;
using DKH.Platform.Authorization.ResourceAccess;
using DKH.Platform.Authorization.ResourceAccess.EntityFrameworkCore.QueryFilters;
using DKH.Platform.EntityFrameworkCore;
using DKH.Platform.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DKH.ApiManagementService.Application.Features.AiProviders.Queries.ListAiProviders;

public sealed class ListAiProvidersQueryHandler(
    IAppDbContext dbContext,
    IPlatformCurrentUser currentUser) : IRequestHandler<ListAiProvidersQuery, ListAiProvidersResult>
{
    public async Task<ListAiProvidersResult> Handle(ListAiProvidersQuery request, CancellationToken cancellationToken)
    {
        var pageSize = request.PageSize is <= 0 or > 200 ? 50 : request.PageSize;
        var page = request.Page > 0 ? request.Page : 1;

        var ef = (DbContext)dbContext;
        var query = dbContext.AiProviders
            .AsNoTracking()
            .ApplySoftDeleteFilter(request.SoftDeleteFilter)
            .ApplyResourceAccessFilter<AiProviderEntity, ApiManagementAccessGrantEntity, Guid>(
                ef,
                currentUser,
                resourceType: "ai_provider",
                permission: ResourceAccessPermissions.Read,
                resourceIdSelector: x => x.Id);

        if (request.TypeFilter.HasValue)
        {
            query = query.Where(x => x.ProviderType == request.TypeFilter.Value);
        }

        if (request.StatusFilter.HasValue)
        {
            query = query.Where(x => x.Status == request.StatusFilter.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var protos = items.Select(e => e.ToProto()).ToList();

        return new ListAiProvidersResult(protos, totalCount);
    }
}
