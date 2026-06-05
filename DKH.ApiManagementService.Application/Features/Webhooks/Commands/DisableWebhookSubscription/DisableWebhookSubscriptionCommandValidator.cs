using FluentValidation;

namespace DKH.ApiManagementService.Application.Features.Webhooks.Commands.DisableWebhookSubscription;

public sealed class DisableWebhookSubscriptionCommandValidator : AbstractValidator<DisableWebhookSubscriptionCommand>
{
    public DisableWebhookSubscriptionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ApiKeyId).NotEmpty();
    }
}
