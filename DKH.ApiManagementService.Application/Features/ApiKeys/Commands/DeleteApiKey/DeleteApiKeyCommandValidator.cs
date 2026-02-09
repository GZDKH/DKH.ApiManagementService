using FluentValidation;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Commands.DeleteApiKey;

public sealed class DeleteApiKeyCommandValidator : AbstractValidator<DeleteApiKeyCommand>
{
    public DeleteApiKeyCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
