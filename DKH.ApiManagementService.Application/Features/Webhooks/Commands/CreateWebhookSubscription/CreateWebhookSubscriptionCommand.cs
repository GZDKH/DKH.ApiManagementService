using MediatR;

namespace DKH.ApiManagementService.Application.Features.Webhooks.Commands.CreateWebhookSubscription;

public sealed record CreateWebhookSubscriptionCommand(
    string Name,
    string CallbackUrl,
    IReadOnlyList<string> Events,
    string SigningSecret,
    int RetryMaxAttempts,
    int RetryBackoffSeconds,
    bool DlqEnabled,
    Guid? ApiKeyId,
    Guid? CustomerId) : IRequest<CreateWebhookSubscriptionResult>;

public sealed record CreateWebhookSubscriptionResult(WebhookSubscriptionDto Subscription);
