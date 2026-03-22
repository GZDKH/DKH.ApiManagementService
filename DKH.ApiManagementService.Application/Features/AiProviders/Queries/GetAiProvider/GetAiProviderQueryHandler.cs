using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Application.Features.AiProviders.Mappers;
using DKH.ApiManagementService.Contracts.ApiManagement.Models.AiProvider.v1;
using Grpc.Core;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.AiProviders.Queries.GetAiProvider;

public sealed class GetAiProviderQueryHandler(
    IAiProviderRepository repository) : IRequestHandler<GetAiProviderQuery, AiProviderModel>
{
    public async Task<AiProviderModel> Handle(GetAiProviderQuery request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound,
                $"AI provider with id '{request.Id}' not found."));

        return entity.ToProto();
    }
}
