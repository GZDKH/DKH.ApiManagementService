using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Application.Features.ApiKeys.Mappers;
using DKH.ApiManagementService.Contracts.Models.V1;
using Grpc.Core;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Commands.UpdateApiKey;

public sealed class UpdateApiKeyCommandHandler(IApiKeyRepository repository) : IRequestHandler<UpdateApiKeyCommand, ApiKey>
{
    public async Task<ApiKey> Handle(UpdateApiKeyCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"API key {request.Id} not found"));

        entity.Update(request.Name, request.Description, request.Permissions, request.ExpiresAt);
        await repository.UpdateAsync(entity, cancellationToken);

        return entity.ToProto();
    }
}
