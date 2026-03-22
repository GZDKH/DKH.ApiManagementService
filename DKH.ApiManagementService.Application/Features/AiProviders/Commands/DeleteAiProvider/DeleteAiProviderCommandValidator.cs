using FluentValidation;

namespace DKH.ApiManagementService.Application.Features.AiProviders.Commands.DeleteAiProvider;

public sealed class DeleteAiProviderCommandValidator : AbstractValidator<DeleteAiProviderCommand>
{
    public DeleteAiProviderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
