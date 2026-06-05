using MediatR;

namespace DKH.ApiManagementService.Application.Features.Webhooks.Commands.UpdateWebhookSubscription;

public sealed record UpdateWebhookSubscriptionCommand(
    Guid Id,
    string Name,
    string CallbackUrl,
    IReadOnlyList<string> Events,
    int RetryMaxAttempts,
    int RetryBackoffSeconds,
    bool DlqEnabled,
    Guid? ApiKeyId,
    Guid? CustomerId) : IRequest<UpdateWebhookSubscriptionResult>;

public sealed record UpdateWebhookSubscriptionResult(WebhookSubscriptionDto Subscription);
