using DKH.ApiManagementService.Contracts.ApiManagement.Models.AiProvider.v1;
using DKH.Platform.Domain.Enums;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.AiProviders.Queries.ListAiProviders;

public sealed record ListAiProvidersQuery(
    int Page,
    int PageSize,
    Domain.Enums.AiProviderType? TypeFilter = null,
    Domain.Enums.AiProviderStatus? StatusFilter = null,
    PlatformSoftDeleteFilter SoftDeleteFilter = PlatformSoftDeleteFilter.ActiveOnly) : IRequest<ListAiProvidersResult>;

public sealed record ListAiProvidersResult(IReadOnlyList<AiProviderModel> Providers, int TotalCount);
