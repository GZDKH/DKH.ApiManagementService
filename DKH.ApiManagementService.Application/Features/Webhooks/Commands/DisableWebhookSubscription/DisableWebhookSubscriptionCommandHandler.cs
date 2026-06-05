using DKH.ApiManagementService.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DKH.ApiManagementService.Application.Features.Webhooks.Commands.DisableWebhookSubscription;

public sealed class DisableWebhookSubscriptionCommandHandler(IAppDbContext dbContext)
    : IRequestHandler<DisableWebhookSubscriptionCommand>
{
    public async Task Handle(DisableWebhookSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.WebhookSubscriptions
            .ForOwner(request.ApiKeyId, request.CustomerId)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Webhook subscription '{request.Id}' was not found.");

        entity.Disable();
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
