using FluentValidation;

namespace DKH.ApiManagementService.Application.Features.AiProviders.Commands.PermanentlyDeleteAiProvider;

public sealed class PermanentlyDeleteAiProviderCommandValidator : AbstractValidator<PermanentlyDeleteAiProviderCommand>
{
    public PermanentlyDeleteAiProviderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
