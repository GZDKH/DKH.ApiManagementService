using FluentValidation;

namespace DKH.ApiManagementService.Application.Features.AiProviders.Commands.RestoreAiProvider;

public sealed class RestoreAiProviderCommandValidator : AbstractValidator<RestoreAiProviderCommand>
{
    public RestoreAiProviderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
