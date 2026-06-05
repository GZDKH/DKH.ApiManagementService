using MediatR;

namespace DKH.ApiManagementService.Application.Features.Webhooks.Queries.GetWebhookSubscription;

public sealed record GetWebhookSubscriptionQuery(
    Guid Id,
    Guid? ApiKeyId,
    Guid? CustomerId) : IRequest<GetWebhookSubscriptionResult>;

public sealed record GetWebhookSubscriptionResult(WebhookSubscriptionDto? Subscription);
