using FluentValidation;

namespace DKH.ApiManagementService.Application.Features.AiProviders.Commands.UpdateAiProvider;

public sealed class UpdateAiProviderCommandValidator : AbstractValidator<UpdateAiProviderCommand>
{
    public UpdateAiProviderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .MaximumLength(256)
            .When(x => x.Name is not null);

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
