using DKH.ApiManagementService.Application.Features.Usage.GetUsageHistory;
using DKH.ApiManagementService.Application.Features.Usage.GetUsageStats;
using DKH.ApiManagementService.Application.Features.Usage.RecordUsage;
using DKH.ApiManagementService.Contracts.Services.V1;
using Grpc.Core;
using MediatR;

namespace DKH.ApiManagementService.Api.Grpc.Services;

public class ApiKeyUsageGrpcService(IMediator mediator) : ApiKeyUsageService.ApiKeyUsageServiceBase
{
    public override async Task<RecordUsageResponse> RecordUsage(RecordUsageRequest request, ServerCallContext context)
    {
        var success = await mediator.Send(
            new RecordUsageCommand(
                Guid.Parse(request.ApiKeyId),
                request.Endpoint,
                request.StatusCode,
                string.IsNullOrWhiteSpace(request.IpAddress) ? null : request.IpAddress,
                string.IsNullOrWhiteSpace(request.UserAgent) ? null : request.UserAgent,
                request.ResponseTimeMs),
            context.CancellationToken);

        return new RecordUsageResponse { Success = success };
    }

    public override async Task<GetUsageStatsResponse> GetUsageStats(GetUsageStatsRequest request, ServerCallContext context)
    {
        var stats = await mediator.Send(
            new GetUsageStatsQuery(
                Guid.Parse(request.ApiKeyId),
                request.From.ToDateTimeOffset(),
                request.To.ToDateTimeOffset()),
            context.CancellationToken);

        return new GetUsageStatsResponse { Stats = stats };
    }

    public override async Task<GetUsageHistoryResponse> GetUsageHistory(GetUsageHistoryRequest request, ServerCallContext context)
    {
        var result = await mediator.Send(
            new GetUsageHistoryQuery(
                Guid.Parse(request.ApiKeyId),
                request.From.ToDateTimeOffset(),
                request.To.ToDateTimeOffset(),
                request.Page > 0 ? request.Page : 1,
                request.PageSize > 0 ? request.PageSize : 20),
            context.CancellationToken);

        var response = new GetUsageHistoryResponse { TotalCount = result.TotalCount };
        response.Records.AddRange(result.Records);
        return response;
    }
}
