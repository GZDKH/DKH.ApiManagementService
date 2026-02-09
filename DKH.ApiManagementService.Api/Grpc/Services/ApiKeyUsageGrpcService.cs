using DKH.ApiManagementService.Contracts.Services.V1;
using Grpc.Core;
using MediatR;

namespace DKH.ApiManagementService.Api.Grpc.Services;

public class ApiKeyUsageGrpcService(IMediator mediator) : ApiKeyUsageService.ApiKeyUsageServiceBase
{
    public override Task<RecordUsageResponse> RecordUsage(RecordUsageRequest request, ServerCallContext context)
    {
        _ = mediator;
        throw new RpcException(new Status(StatusCode.Unimplemented, "Not yet implemented"));
    }

    public override Task<GetUsageStatsResponse> GetUsageStats(GetUsageStatsRequest request, ServerCallContext context)
    {
        throw new RpcException(new Status(StatusCode.Unimplemented, "Not yet implemented"));
    }

    public override Task<GetUsageHistoryResponse> GetUsageHistory(GetUsageHistoryRequest request, ServerCallContext context)
    {
        throw new RpcException(new Status(StatusCode.Unimplemented, "Not yet implemented"));
    }
}
