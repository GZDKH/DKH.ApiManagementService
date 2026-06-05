using DKH.ApiManagementService.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DKH.ApiManagementService.Application.Features.Webhooks.Commands.RotateWebhookSubscriptionSecret;

public sealed class RotateWebhookSubscriptionSecretCommandHandler(IAppDbContext dbContext)
    : IRequestHandler<RotateWebhookSubscriptionSecretCommand, RotateWebhookSubscriptionSecretResult>
{
    public async Task<RotateWebhookSubscriptionSecretResult> Handle(
        RotateWebhookSubscriptionSecretCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.WebhookSubscriptions
            .ForOwner(request.ApiKeyId, request.CustomerId)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Webhook subscription '{request.Id}' was not found.");

        entity.RotateSecret(request.SigningSecret);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new RotateWebhookSubscriptionSecretResult(entity.ToDto());
    }
}
