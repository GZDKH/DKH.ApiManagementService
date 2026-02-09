using FluentValidation;

namespace DKH.ApiManagementService.Application.Features.Usage.GetUsageHistory;

public sealed class GetUsageHistoryQueryValidator : AbstractValidator<GetUsageHistoryQuery>
{
    public GetUsageHistoryQueryValidator()
    {
        RuleFor(x => x.ApiKeyId)
            .NotEmpty();

        RuleFor(x => x.From)
            .LessThan(x => x.To);

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
