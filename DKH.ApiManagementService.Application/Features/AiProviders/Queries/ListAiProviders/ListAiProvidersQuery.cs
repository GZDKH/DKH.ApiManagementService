using DKH.ApiManagementService.Contracts.ApiManagement.Models.AiProvider.v1;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.AiProviders.Queries.ListAiProviders;

public sealed record ListAiProvidersQuery(
    int Page,
    int PageSize,
    Domain.Enums.AiProviderType? TypeFilter = null,
    Domain.Enums.AiProviderStatus? StatusFilter = null) : IRequest<ListAiProvidersResult>;

public sealed record ListAiProvidersResult(IReadOnlyList<AiProviderModel> Providers, int TotalCount);
