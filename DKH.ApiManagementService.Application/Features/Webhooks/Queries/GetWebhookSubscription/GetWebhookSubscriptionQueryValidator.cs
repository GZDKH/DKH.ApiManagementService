using FluentValidation;

namespace DKH.ApiManagementService.Application.Features.Webhooks.Queries.GetWebhookSubscription;

public sealed class GetWebhookSubscriptionQueryValidator : AbstractValidator<GetWebhookSubscriptionQuery>
{
    public GetWebhookSubscriptionQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ApiKeyId).NotEmpty();
    }
}
