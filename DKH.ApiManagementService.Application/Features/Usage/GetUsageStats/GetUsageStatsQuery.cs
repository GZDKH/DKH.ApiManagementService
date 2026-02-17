using DKH.ApiManagementService.Contracts.ApiManagement.Models.ApiKeyUsage.v1;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.Usage.GetUsageStats;

public sealed record GetUsageStatsQuery(
    Guid ApiKeyId,
    DateTimeOffset From,
    DateTimeOffset To) : IRequest<ApiKeyUsageStatsModel>;
