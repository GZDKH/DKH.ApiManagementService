using DKH.ApiManagementService.Contracts.Services.V1;
using Grpc.Core;
using MediatR;

namespace DKH.ApiManagementService.Api.Grpc.Services;

public class ApiKeyCrudGrpcService(IMediator mediator) : ApiKeyCrudService.ApiKeyCrudServiceBase
{
    public override Task<CreateApiKeyResponse> CreateApiKey(CreateApiKeyRequest request, ServerCallContext context)
    {
        _ = mediator;
        throw new RpcException(new Status(StatusCode.Unimplemented, "Not yet implemented"));
    }

    public override Task<GetApiKeyResponse> GetApiKey(GetApiKeyRequest request, ServerCallContext context)
    {
        throw new RpcException(new Status(StatusCode.Unimplemented, "Not yet implemented"));
    }

    public override Task<ListApiKeysResponse> ListApiKeys(ListApiKeysRequest request, ServerCallContext context)
    {
        throw new RpcException(new Status(StatusCode.Unimplemented, "Not yet implemented"));
    }

    public override Task<UpdateApiKeyResponse> UpdateApiKey(UpdateApiKeyRequest request, ServerCallContext context)
    {
        throw new RpcException(new Status(StatusCode.Unimplemented, "Not yet implemented"));
    }

    public override Task<DeleteApiKeyResponse> DeleteApiKey(DeleteApiKeyRequest request, ServerCallContext context)
    {
        throw new RpcException(new Status(StatusCode.Unimplemented, "Not yet implemented"));
    }

    public override Task<RegenerateApiKeyResponse> RegenerateApiKey(RegenerateApiKeyRequest request, ServerCallContext context)
    {
        throw new RpcException(new Status(StatusCode.Unimplemented, "Not yet implemented"));
    }
}
