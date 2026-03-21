using DKH.ApiManagementService.Contracts.ApiManagement.Models.ApiKey.v1;
using DKH.Platform.Domain.Enums;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Queries.ListApiKeys;

public sealed record ListApiKeysQuery(
    int Page,
    int PageSize,
    Domain.Enums.ApiKeyScope? ScopeFilter = null,
    Domain.Enums.ApiKeyStatus? StatusFilter = null,
    PlatformSoftDeleteFilter SoftDeleteFilter = PlatformSoftDeleteFilter.ActiveOnly) : IRequest<ListApiKeysResult>;

public sealed record ListApiKeysResult(IReadOnlyList<ApiKeyModel> ApiKeys, int TotalCount);
