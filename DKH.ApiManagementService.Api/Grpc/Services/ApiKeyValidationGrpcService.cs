using DKH.ApiManagementService.Application.Features.ApiKeys.Mappers;
using DKH.ApiManagementService.Application.Features.Validation.CheckPermission;
using DKH.ApiManagementService.Application.Features.Validation.ValidateApiKey;
using DKH.ApiManagementService.Contracts.Services.V1;
using Grpc.Core;
using MediatR;

namespace DKH.ApiManagementService.Api.Grpc.Services;

public class ApiKeyValidationGrpcService(IMediator mediator) : ApiKeyValidationService.ApiKeyValidationServiceBase
{
    public override async Task<ValidateApiKeyResponse> ValidateApiKey(ValidateApiKeyRequest request, ServerCallContext context)
    {
        var result = await mediator.Send(
            new ValidateApiKeyQuery(request.RawKey),
            context.CancellationToken);

        var response = new ValidateApiKeyResponse
        {
            IsValid = result.IsValid,
            ApiKeyId = result.ApiKeyId?.ToString() ?? string.Empty,
            ErrorReason = result.ErrorReason ?? string.Empty,
        };

        if (result.Scope.HasValue)
        {
            response.Scope = result.Scope.Value.ToProtoScope();
        }

        if (result.Permissions is not null)
        {
            response.Permissions.AddRange(result.Permissions);
        }

        return response;
    }

    public override async Task<CheckPermissionResponse> CheckPermission(CheckPermissionRequest request, ServerCallContext context)
    {
        var result = await mediator.Send(
            new CheckPermissionQuery(request.RawKey, request.RequiredPermission),
            context.CancellationToken);

        return new CheckPermissionResponse
        {
            IsAllowed = result.IsAllowed,
            ErrorReason = result.ErrorReason ?? string.Empty,
        };
    }
}
