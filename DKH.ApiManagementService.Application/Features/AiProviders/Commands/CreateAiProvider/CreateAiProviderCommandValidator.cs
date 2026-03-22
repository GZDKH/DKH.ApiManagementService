using FluentValidation;

namespace DKH.ApiManagementService.Application.Features.AiProviders.Commands.CreateAiProvider;

public sealed class CreateAiProviderCommandValidator : AbstractValidator<CreateAiProviderCommand>
{
    public CreateAiProviderCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.ProviderType)
            .IsInEnum();

        RuleFor(x => x.DisplayName)
            .MaximumLength(256)
            .When(x => x.DisplayName is not null);

        RuleFor(x => x.BaseUrl)
            .MaximumLength(2048)
            .When(x => x.BaseUrl is not null);

        RuleFor(x => x.RateLimitPerMinute)
            .GreaterThan(0)
            .When(x => x.RateLimitPerMinute.HasValue);

        RuleFor(x => x.DailyQuota)
            .GreaterThan(0)
            .When(x => x.DailyQuota.HasValue);
    }
}
