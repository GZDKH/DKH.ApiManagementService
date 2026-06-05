using DKH.ApiManagementService.Domain.Entities;

namespace DKH.ApiManagementService.Application.Features.Webhooks;

internal static class WebhookSubscriptionQueryFilters
{
    public static IQueryable<WebhookSubscriptionEntity> ForOwner(
        this IQueryable<WebhookSubscriptionEntity> query,
        Guid? apiKeyId,
        Guid? customerId)
    {
        if (!apiKeyId.HasValue)
        {
            return query.Where(_ => false);
        }

        query = query.Where(x => x.ApiKeyId == apiKeyId.Value);

        if (customerId.HasValue)
        {
            query = query.Where(x => x.CustomerId == customerId.Value);
        }

        return query;
    }
}
