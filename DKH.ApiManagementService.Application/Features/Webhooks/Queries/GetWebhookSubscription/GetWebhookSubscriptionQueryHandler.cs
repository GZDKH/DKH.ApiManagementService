using DKH.ApiManagementService.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DKH.ApiManagementService.Application.Features.Webhooks.Queries.GetWebhookSubscription;

public sealed class GetWebhookSubscriptionQueryHandler(IAppDbContext dbContext)
    : IRequestHandler<GetWebhookSubscriptionQuery, GetWebhookSubscriptionResult>
{
    public async Task<GetWebhookSubscriptionResult> Handle(
        GetWebhookSubscriptionQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.WebhookSubscriptions
            .AsNoTracking()
            .ForOwner(request.ApiKeyId, request.CustomerId)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        return new GetWebhookSubscriptionResult(entity?.ToDto());
    }
}
