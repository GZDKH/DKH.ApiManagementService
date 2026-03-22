using FluentValidation;

namespace DKH.ApiManagementService.Application.Features.AiProviders.Queries.GetAiProvider;

public sealed class GetAiProviderQueryValidator : AbstractValidator<GetAiProviderQuery>
{
    public GetAiProviderQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
