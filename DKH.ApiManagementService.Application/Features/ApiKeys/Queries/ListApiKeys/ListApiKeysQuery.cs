using DKH.ApiManagementService.Contracts.ApiManagement.Models.ApiKey.v1;
using DKH.Platform.Domain.Enums;
using MediatR;
using DomainEnvironment = DKH.ApiManagementService.Domain.Enums.ApiKeyEnvironment;
using DomainRateLimitTier = DKH.ApiManagementService.Domain.Enums.ApiKeyRateLimitTier;
using DomainScope = DKH.ApiManagementService.Domain.Enums.ApiKeyScope;
using DomainStatus = DKH.ApiManagementService.Domain.Enums.ApiKeyStatus;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Queries.ListApiKeys;

public sealed record ListApiKeysQuery(
    int Page,
    int PageSize,
    DomainScope? ScopeFilter = null,
    DomainStatus? StatusFilter = null,
    Guid? CustomerIdFilter = null,
    DomainEnvironment? EnvironmentFilter = null,
    DomainRateLimitTier? RateLimitTierFilter = null,
    PlatformSoftDeleteFilter SoftDeleteFilter = PlatformSoftDeleteFilter.ActiveOnly) : IRequest<ListApiKeysResult>;

public sealed record ListApiKeysResult(IReadOnlyList<ApiKeyModel> ApiKeys, int TotalCount);
