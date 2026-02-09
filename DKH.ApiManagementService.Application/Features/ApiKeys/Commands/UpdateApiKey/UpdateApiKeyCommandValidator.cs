using FluentValidation;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Commands.UpdateApiKey;

public sealed class UpdateApiKeyCommandValidator : AbstractValidator<UpdateApiKeyCommand>
{
    public UpdateApiKeyCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .MaximumLength(256)
            .When(x => x.Name is not null);

        RuleFor(x => x.Description)
            .MaximumLength(1024)
            .When(x => x.Description is not null);
    }
}
