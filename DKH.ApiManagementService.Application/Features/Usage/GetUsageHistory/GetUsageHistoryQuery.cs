using DKH.ApiManagementService.Contracts.ApiManagement.Models.ApiKeyUsage.v1;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.Usage.GetUsageHistory;

public sealed record GetUsageHistoryQuery(
    Guid ApiKeyId,
    DateTimeOffset From,
    DateTimeOffset To,
    int Page,
    int PageSize) : IRequest<GetUsageHistoryResult>;

public sealed record GetUsageHistoryResult(IReadOnlyList<ApiKeyUsageModel> Records, int TotalCount);
