using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Application.Features.ApiKeys.Mappers;
using Grpc.Core;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Commands.RegenerateApiKey;

public sealed class RegenerateApiKeyCommandHandler(
    IApiKeyRepository repository,
    IApiKeyGenerator keyGenerator) : IRequestHandler<RegenerateApiKeyCommand, RegenerateApiKeyResult>
{
    public async Task<RegenerateApiKeyResult> Handle(RegenerateApiKeyCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"API key {request.Id} not found"));

        var (rawKey, keyHash, keyPrefix) = keyGenerator.Generate(entity.Scope);
        entity.Regenerate(keyHash, keyPrefix);
        await repository.UpdateAsync(entity, cancellationToken);

        return new RegenerateApiKeyResult(entity.ToProto(), rawKey);
    }
}
