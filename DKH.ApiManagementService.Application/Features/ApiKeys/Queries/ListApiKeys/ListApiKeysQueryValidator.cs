using FluentValidation;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Queries.ListApiKeys;

public sealed class ListApiKeysQueryValidator : AbstractValidator<ListApiKeysQuery>
{
    public ListApiKeysQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
