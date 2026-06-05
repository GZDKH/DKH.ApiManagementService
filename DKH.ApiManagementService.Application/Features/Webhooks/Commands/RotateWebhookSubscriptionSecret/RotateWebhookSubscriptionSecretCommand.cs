using MediatR;

namespace DKH.ApiManagementService.Application.Features.Webhooks.Commands.RotateWebhookSubscriptionSecret;

public sealed record RotateWebhookSubscriptionSecretCommand(
    Guid Id,
    string SigningSecret,
    Guid? ApiKeyId,
    Guid? CustomerId) : IRequest<RotateWebhookSubscriptionSecretResult>;

public sealed record RotateWebhookSubscriptionSecretResult(WebhookSubscriptionDto Subscription);
