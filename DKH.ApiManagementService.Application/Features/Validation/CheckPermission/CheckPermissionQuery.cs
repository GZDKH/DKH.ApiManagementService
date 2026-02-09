using MediatR;

namespace DKH.ApiManagementService.Application.Features.Validation.CheckPermission;

public sealed record CheckPermissionQuery(string RawKey, string RequiredPermission) : IRequest<CheckPermissionResult>;

public sealed record CheckPermissionResult(bool IsAllowed, string? ErrorReason = null);
