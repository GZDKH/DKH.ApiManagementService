using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Application.Features.ApiKeys.Mappers;
using DKH.ApiManagementService.Contracts.ApiManagement.Models.ApiKey.v1;
using Grpc.Core;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Queries.GetApiKey;

public sealed class GetApiKeyQueryHandler(IApiKeyRepository repository) : IRequestHandler<GetApiKeyQuery, ApiKeyModel>
{
    public async Task<ApiKeyModel> Handle(GetApiKeyQuery request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"API key {request.Id} not found"));

        return entity.ToProto();
    }
}
