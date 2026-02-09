using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Application.Features.ApiKeys.Mappers;
using DKH.ApiManagementService.Domain.Entities;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Commands.CreateApiKey;

public sealed class CreateApiKeyCommandHandler(
    IApiKeyRepository repository,
    IApiKeyGenerator keyGenerator) : IRequestHandler<CreateApiKeyCommand, CreateApiKeyResult>
{
    public async Task<CreateApiKeyResult> Handle(CreateApiKeyCommand request, CancellationToken cancellationToken)
    {
        var (rawKey, keyHash, keyPrefix) = keyGenerator.Generate(request.Scope);

        var entity = new ApiKeyEntity(
            request.Name,
            keyHash,
            keyPrefix,
            request.Scope,
            request.Permissions,
            request.CreatedBy,
            request.Description,
            request.ExpiresAt);

        await repository.AddAsync(entity, cancellationToken);

        return new CreateApiKeyResult(entity.ToProto(), rawKey);
    }
}
