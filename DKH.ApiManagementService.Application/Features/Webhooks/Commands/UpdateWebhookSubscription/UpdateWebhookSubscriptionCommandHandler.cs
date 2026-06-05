using DKH.ApiManagementService.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DKH.ApiManagementService.Application.Features.Webhooks.Commands.UpdateWebhookSubscription;

public sealed class UpdateWebhookSubscriptionCommandHandler(IAppDbContext dbContext)
    : IRequestHandler<UpdateWebhookSubscriptionCommand, UpdateWebhookSubscriptionResult>
{
    public async Task<UpdateWebhookSubscriptionResult> Handle(
        UpdateWebhookSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.WebhookSubscriptions
            .ForOwner(request.ApiKeyId, request.CustomerId)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Webhook subscription '{request.Id}' was not found.");

        entity.Update(
            request.Name,
            request.CallbackUrl,
            request.Events,
            request.RetryMaxAttempts,
            request.RetryBackoffSeconds,
            request.DlqEnabled);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateWebhookSubscriptionResult(entity.ToDto());
    }
}
