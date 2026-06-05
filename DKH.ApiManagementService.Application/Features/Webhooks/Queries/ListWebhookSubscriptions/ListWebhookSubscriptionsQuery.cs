using MediatR;

namespace DKH.ApiManagementService.Application.Features.Webhooks.Queries.ListWebhookSubscriptions;

public sealed record ListWebhookSubscriptionsQuery(
    Guid? ApiKeyId,
    Guid? CustomerId,
    int Page = 1,
    int PageSize = 20) : IRequest<ListWebhookSubscriptionsResult>;

public sealed record ListWebhookSubscriptionsResult(
    IReadOnlyList<WebhookSubscriptionDto> Subscriptions,
    int TotalCount);
