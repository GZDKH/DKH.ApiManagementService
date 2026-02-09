using FluentValidation;

namespace DKH.ApiManagementService.Application.Features.Usage.GetUsageStats;

public sealed class GetUsageStatsQueryValidator : AbstractValidator<GetUsageStatsQuery>
{
    public GetUsageStatsQueryValidator()
    {
        RuleFor(x => x.ApiKeyId)
            .NotEmpty();

        RuleFor(x => x.From)
            .LessThan(x => x.To);
    }
}
