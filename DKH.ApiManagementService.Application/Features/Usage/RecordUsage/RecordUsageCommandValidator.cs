using FluentValidation;

namespace DKH.ApiManagementService.Application.Features.Usage.RecordUsage;

public sealed class RecordUsageCommandValidator : AbstractValidator<RecordUsageCommand>
{
    public RecordUsageCommandValidator()
    {
        RuleFor(x => x.ApiKeyId)
            .NotEmpty();

        RuleFor(x => x.Endpoint)
            .NotEmpty()
            .MaximumLength(512);
    }
}
