using DKH.ApiManagementService.Contracts.Models.V1;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.Usage.GetUsageStats;

public sealed record GetUsageStatsQuery(
    Guid ApiKeyId,
    DateTimeOffset From,
    DateTimeOffset To) : IRequest<ApiKeyUsageStats>;
