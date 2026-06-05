using FluentValidation;

namespace DKH.ApiManagementService.Application.Features.Webhooks.Queries.ListWebhookSubscriptions;

public sealed class ListWebhookSubscriptionsQueryValidator : AbstractValidator<ListWebhookSubscriptionsQuery>
{
    public ListWebhookSubscriptionsQueryValidator()
    {
        RuleFor(x => x.ApiKeyId).NotEmpty();
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
