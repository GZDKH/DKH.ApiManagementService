using FluentValidation;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Commands.CreateApiKey;

public sealed class CreateApiKeyCommandValidator : AbstractValidator<CreateApiKeyCommand>
{
    public CreateApiKeyCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.Scope)
            .IsInEnum();

        RuleFor(x => x.Environment)
            .IsInEnum();

        RuleFor(x => x.RateLimitTier)
            .IsInEnum();

        RuleFor(x => x.Description)
            .MaximumLength(1024)
            .When(x => x.Description is not null);

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTimeOffset.UtcNow)
            .When(x => x.ExpiresAt.HasValue);
    }
}
