using FluentValidation;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Queries.GetApiKey;

public sealed class GetApiKeyQueryValidator : AbstractValidator<GetApiKeyQuery>
{
    public GetApiKeyQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
