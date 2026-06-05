using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Domain.Entities;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.Webhooks.Commands.CreateWebhookSubscription;

public sealed class CreateWebhookSubscriptionCommandHandler(IAppDbContext dbContext)
    : IRequestHandler<CreateWebhookSubscriptionCommand, CreateWebhookSubscriptionResult>
{
    public async Task<CreateWebhookSubscriptionResult> Handle(
        CreateWebhookSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        if (!request.ApiKeyId.HasValue)
        {
            throw new UnauthorizedAccessException("Webhook subscriptions require an API key owner context.");
        }

        var entity = WebhookSubscriptionEntity.Create(
            request.ApiKeyId,
            request.CustomerId,
            request.Name,
            request.CallbackUrl,
            request.Events,
            request.SigningSecret,
            request.RetryMaxAttempts,
            request.RetryBackoffSeconds,
            request.DlqEnabled);

        dbContext.WebhookSubscriptions.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateWebhookSubscriptionResult(entity.ToDto());
    }
}
