using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Application.Features.AiProviders.Mappers;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.AiProviders.Queries.ListAiProviders;

public sealed class ListAiProvidersQueryHandler(
    IAiProviderRepository repository) : IRequestHandler<ListAiProvidersQuery, ListAiProvidersResult>
{
    public async Task<ListAiProvidersResult> Handle(ListAiProvidersQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await repository.ListAsync(
            request.Page,
            request.PageSize,
            request.TypeFilter,
            request.StatusFilter,
            cancellationToken);

        var protos = items.Select(e => e.ToProto()).ToList();

        return new ListAiProvidersResult(protos, totalCount);
    }
}
