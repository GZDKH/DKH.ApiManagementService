using FluentValidation;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Commands.RegenerateApiKey;

public sealed class RegenerateApiKeyCommandValidator : AbstractValidator<RegenerateApiKeyCommand>
{
    public RegenerateApiKeyCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
