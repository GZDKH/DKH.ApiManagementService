using DKH.ApiManagementService.Contracts.Models.V1;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.Usage.GetUsageHistory;

public sealed record GetUsageHistoryQuery(
    Guid ApiKeyId,
    DateTimeOffset From,
    DateTimeOffset To,
    int Page,
    int PageSize) : IRequest<GetUsageHistoryResult>;

public sealed record GetUsageHistoryResult(IReadOnlyList<ApiKeyUsage> Records, int TotalCount);
