using FluentValidation;

namespace DKH.ApiManagementService.Application.Features.Webhooks.Commands.UpdateWebhookSubscription;

public sealed class UpdateWebhookSubscriptionCommandValidator : AbstractValidator<UpdateWebhookSubscriptionCommand>
{
    public UpdateWebhookSubscriptionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ApiKeyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.CallbackUrl).NotEmpty().MaximumLength(2048).Must(BeAbsoluteHttpsUrl);
        RuleFor(x => x.Events).NotEmpty();
        RuleForEach(x => x.Events).NotEmpty().MaximumLength(128);
        RuleFor(x => x.RetryMaxAttempts).InclusiveBetween(1, 20);
        RuleFor(x => x.RetryBackoffSeconds).InclusiveBetween(1, 86_400);
    }

    private static bool BeAbsoluteHttpsUrl(string callbackUrl)
    {
        return Uri.TryCreate(callbackUrl, UriKind.Absolute, out var uri) &&
               uri.Scheme == Uri.UriSchemeHttps;
    }
}
