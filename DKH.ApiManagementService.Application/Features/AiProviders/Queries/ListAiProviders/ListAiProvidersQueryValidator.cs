using FluentValidation;

namespace DKH.ApiManagementService.Application.Features.AiProviders.Queries.ListAiProviders;

public sealed class ListAiProvidersQueryValidator : AbstractValidator<ListAiProvidersQuery>
{
    public ListAiProvidersQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);
    }
}
