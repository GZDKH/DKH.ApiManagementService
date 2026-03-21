using FluentValidation;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Commands.RestoreApiKey;

public sealed class RestoreApiKeyCommandValidator : AbstractValidator<RestoreApiKeyCommand>
{
    public RestoreApiKeyCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
