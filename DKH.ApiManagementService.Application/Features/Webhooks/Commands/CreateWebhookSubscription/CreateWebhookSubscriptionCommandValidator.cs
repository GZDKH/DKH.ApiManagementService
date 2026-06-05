using FluentValidation;

namespace DKH.ApiManagementService.Application.Features.Webhooks.Commands.CreateWebhookSubscription;

public sealed class CreateWebhookSubscriptionCommandValidator : AbstractValidator<CreateWebhookSubscriptionCommand>
{
    public CreateWebhookSubscriptionCommandValidator()
    {
        RuleFor(x => x.ApiKeyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.CallbackUrl).NotEmpty().MaximumLength(2048).Must(BeAbsoluteHttpsUrl);
        RuleFor(x => x.Events).NotEmpty();
        RuleForEach(x => x.Events).NotEmpty().MaximumLength(128);
        RuleFor(x => x.SigningSecret).NotEmpty().MinimumLength(16).MaximumLength(512);
        RuleFor(x => x.RetryMaxAttempts).InclusiveBetween(1, 20);
        RuleFor(x => x.RetryBackoffSeconds).InclusiveBetween(1, 86_400);
    }

    private static bool BeAbsoluteHttpsUrl(string callbackUrl)
    {
        return Uri.TryCreate(callbackUrl, UriKind.Absolute, out var uri) &&
               uri.Scheme == Uri.UriSchemeHttps;
    }
}
