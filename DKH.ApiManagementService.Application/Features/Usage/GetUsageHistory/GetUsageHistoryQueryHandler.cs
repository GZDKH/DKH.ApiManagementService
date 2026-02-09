using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Contracts.Models.V1;
using Google.Protobuf.WellKnownTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DKH.ApiManagementService.Application.Features.Usage.GetUsageHistory;

public sealed class GetUsageHistoryQueryHandler(IAppDbContext dbContext) : IRequestHandler<GetUsageHistoryQuery, GetUsageHistoryResult>
{
    public async Task<GetUsageHistoryResult> Handle(GetUsageHistoryQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.ApiKeyUsageRecords
            .AsNoTracking()
            .Where(x => x.ApiKeyId == request.ApiKeyId
                && x.Timestamp >= request.From
                && x.Timestamp <= request.To);

        var totalCount = await query.CountAsync(cancellationToken);

        var entities = await query
            .OrderByDescending(x => x.Timestamp)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var records = entities.Select(e => new ApiKeyUsage
        {
            Id = e.Id.ToString(),
            ApiKeyId = e.ApiKeyId.ToString(),
            Endpoint = e.Endpoint,
            StatusCode = e.StatusCode,
            IpAddress = e.IpAddress,
            UserAgent = e.UserAgent,
            Timestamp = Timestamp.FromDateTimeOffset(e.Timestamp),
            ResponseTimeMs = e.ResponseTimeMs,
        }).ToList();

        return new GetUsageHistoryResult(records, totalCount);
    }
}
