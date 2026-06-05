using MediatR;

namespace DKH.ApiManagementService.Application.Features.Webhooks.Commands.DisableWebhookSubscription;

public sealed record DisableWebhookSubscriptionCommand(
    Guid Id,
    Guid? ApiKeyId,
    Guid? CustomerId) : IRequest;
