using FluentValidation;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Commands.IssueTemporaryScopeToken;

public sealed class IssueTemporaryScopeTokenCommandValidator : AbstractValidator<IssueTemporaryScopeTokenCommand>
{
    private static readonly TimeSpan MinimumTtl = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan MaximumTtl = TimeSpan.FromHours(24);

    public IssueTemporaryScopeTokenCommandValidator()
    {
        RuleFor(x => x.SubjectId)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.ResourceType)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.ResourceId)
            .NotEmpty();

        RuleFor(x => x.Permissions)
            .NotEmpty();

        RuleForEach(x => x.Permissions)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.Ttl)
            .InclusiveBetween(MinimumTtl, MaximumTtl);

        RuleFor(x => x.Reason)
            .MaximumLength(512)
            .When(x => x.Reason is not null);
    }
}
