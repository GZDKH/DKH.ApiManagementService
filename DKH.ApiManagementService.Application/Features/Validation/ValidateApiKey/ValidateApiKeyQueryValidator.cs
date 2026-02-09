using FluentValidation;

namespace DKH.ApiManagementService.Application.Features.Validation.ValidateApiKey;

public sealed class ValidateApiKeyQueryValidator : AbstractValidator<ValidateApiKeyQuery>
{
    public ValidateApiKeyQueryValidator()
    {
        RuleFor(x => x.RawKey)
            .NotEmpty();
    }
}
