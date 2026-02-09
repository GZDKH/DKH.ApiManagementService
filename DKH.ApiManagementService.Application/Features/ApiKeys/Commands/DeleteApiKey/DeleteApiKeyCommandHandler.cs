using DKH.ApiManagementService.Application.Abstractions;
using Grpc.Core;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Commands.DeleteApiKey;

public sealed class DeleteApiKeyCommandHandler(IApiKeyRepository repository) : IRequestHandler<DeleteApiKeyCommand, bool>
{
    public async Task<bool> Handle(DeleteApiKeyCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"API key {request.Id} not found"));

        entity.Revoke();
        await repository.UpdateAsync(entity, cancellationToken);

        return true;
    }
}
