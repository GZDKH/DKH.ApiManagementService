using DKH.ApiManagementService.Contracts.Models.V1;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Queries.ListApiKeys;

public sealed record ListApiKeysQuery(
    int Page,
    int PageSize,
    Domain.Enums.ApiKeyScope? ScopeFilter = null,
    Domain.Enums.ApiKeyStatus? StatusFilter = null) : IRequest<ListApiKeysResult>;

public sealed record ListApiKeysResult(IReadOnlyList<ApiKey> ApiKeys, int TotalCount);
