using FluentValidation;

namespace DKH.ApiManagementService.Application.Features.Webhooks.Commands.RotateWebhookSubscriptionSecret;

public sealed class RotateWebhookSubscriptionSecretCommandValidator : AbstractValidator<RotateWebhookSubscriptionSecretCommand>
{
    public RotateWebhookSubscriptionSecretCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ApiKeyId).NotEmpty();
        RuleFor(x => x.SigningSecret).NotEmpty().MinimumLength(16).MaximumLength(512);
    }
}
