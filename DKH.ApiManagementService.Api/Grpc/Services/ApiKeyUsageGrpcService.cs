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
                request.ApiKeyId,
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
                request.ApiKeyId,
                request.From.ToDateTimeOffset(),
                request.To.ToDateTimeOffset()),
            context.CancellationToken);

        return new GetUsageStatsResponse { Stats = stats };
    }

    public override async Task<GetUsageHistoryResponse> GetUsageHistory(GetUsageHistoryRequest request, ServerCallContext context)
    {
        var result = await mediator.Send(
            new GetUsageHistoryQuery(
                request.ApiKeyId,
                request.From.ToDateTimeOffset(),
                request.To.ToDateTimeOffset(),
                request.Pagination?.Page > 0 ? request.Pagination.Page : 1,
                request.Pagination?.PageSize > 0 ? request.Pagination.PageSize : 20),
            context.CancellationToken);

        var page = request.Pagination?.Page ?? 1;
        var pageSize = request.Pagination?.PageSize ?? 20;
        var totalPages = (int)Math.Ceiling((double)result.TotalCount / pageSize);

        var response = new GetUsageHistoryResponse
        {
            Metadata = new Platform.Grpc.Common.Types.PaginationMetadata
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = result.TotalCount,
                TotalPages = totalPages,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1,
            },
        };
        response.Records.AddRange(result.Records);
        return response;
    }
}
