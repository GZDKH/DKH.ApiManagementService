using DKH.ApiManagementService.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DKH.ApiManagementService.Application.Features.Webhooks.Queries.ListWebhookSubscriptions;

public sealed class ListWebhookSubscriptionsQueryHandler(IAppDbContext dbContext)
    : IRequestHandler<ListWebhookSubscriptionsQuery, ListWebhookSubscriptionsResult>
{
    public async Task<ListWebhookSubscriptionsResult> Handle(
        ListWebhookSubscriptionsQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = dbContext.WebhookSubscriptions
            .AsNoTracking()
            .ForOwner(request.ApiKeyId, request.CustomerId);

        var totalCount = await query.CountAsync(cancellationToken);
        var subscriptions = await query
            .OrderByDescending(x => x.CreationTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => x.ToDto())
            .ToListAsync(cancellationToken);

        return new ListWebhookSubscriptionsResult(subscriptions, totalCount);
    }
}
