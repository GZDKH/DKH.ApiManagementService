using DKH.ApiManagementService.Contracts.Services.V1;
using Grpc.Core;
using MediatR;

namespace DKH.ApiManagementService.Api.Grpc.Services;

public class ApiKeyValidationGrpcService(IMediator mediator) : ApiKeyValidationService.ApiKeyValidationServiceBase
{
    public override Task<ValidateApiKeyResponse> ValidateApiKey(ValidateApiKeyRequest request, ServerCallContext context)
    {
        _ = mediator;
        throw new RpcException(new Status(StatusCode.Unimplemented, "Not yet implemented"));
    }

    public override Task<CheckPermissionResponse> CheckPermission(CheckPermissionRequest request, ServerCallContext context)
    {
        throw new RpcException(new Status(StatusCode.Unimplemented, "Not yet implemented"));
    }
}
