using FluentValidation;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Commands.PermanentlyDeleteApiKey;

public sealed class PermanentlyDeleteApiKeyCommandValidator : AbstractValidator<PermanentlyDeleteApiKeyCommand>
{
    public PermanentlyDeleteApiKeyCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
