using FluentValidation;

namespace DKH.ApiManagementService.Application.Features.Validation.CheckPermission;

public sealed class CheckPermissionQueryValidator : AbstractValidator<CheckPermissionQuery>
{
    public CheckPermissionQueryValidator()
    {
        RuleFor(x => x.RawKey)
            .NotEmpty();

        RuleFor(x => x.RequiredPermission)
            .NotEmpty();
    }
}
