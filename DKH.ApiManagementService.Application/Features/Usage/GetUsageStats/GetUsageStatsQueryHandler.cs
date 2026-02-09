using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Contracts.Models.V1;
using Google.Protobuf.WellKnownTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DKH.ApiManagementService.Application.Features.Usage.GetUsageStats;

public sealed class GetUsageStatsQueryHandler(IAppDbContext dbContext) : IRequestHandler<GetUsageStatsQuery, ApiKeyUsageStats>
{
    public async Task<ApiKeyUsageStats> Handle(GetUsageStatsQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.ApiKeyUsageRecords
            .AsNoTracking()
            .Where(x => x.ApiKeyId == request.ApiKeyId
                && x.Timestamp >= request.From
                && x.Timestamp <= request.To);

        var totalRequests = await query.CountAsync(cancellationToken);
        var successfulRequests = await query.CountAsync(x => x.StatusCode >= 200 && x.StatusCode < 300, cancellationToken);
        var failedRequests = totalRequests - successfulRequests;

        return new ApiKeyUsageStats
        {
            ApiKeyId = request.ApiKeyId.ToString(),
            TotalRequests = totalRequests,
            SuccessfulRequests = successfulRequests,
            FailedRequests = failedRequests,
            PeriodStart = Timestamp.FromDateTimeOffset(request.From),
            PeriodEnd = Timestamp.FromDateTimeOffset(request.To),
        };
    }
}
