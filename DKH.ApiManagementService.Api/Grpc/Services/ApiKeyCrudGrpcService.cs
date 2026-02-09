using DKH.ApiManagementService.Application.Features.ApiKeys.Commands.CreateApiKey;
using DKH.ApiManagementService.Application.Features.ApiKeys.Commands.DeleteApiKey;
using DKH.ApiManagementService.Application.Features.ApiKeys.Commands.RegenerateApiKey;
using DKH.ApiManagementService.Application.Features.ApiKeys.Commands.UpdateApiKey;
using DKH.ApiManagementService.Application.Features.ApiKeys.Mappers;
using DKH.ApiManagementService.Application.Features.ApiKeys.Queries.GetApiKey;
using DKH.ApiManagementService.Application.Features.ApiKeys.Queries.ListApiKeys;
using DKH.ApiManagementService.Contracts.Services.V1;
using Grpc.Core;
using MediatR;

namespace DKH.ApiManagementService.Api.Grpc.Services;

public class ApiKeyCrudGrpcService(IMediator mediator) : ApiKeyCrudService.ApiKeyCrudServiceBase
{
    public override async Task<CreateApiKeyResponse> CreateApiKey(CreateApiKeyRequest request, ServerCallContext context)
    {
        var result = await mediator.Send(
            new CreateApiKeyCommand(
                request.Name,
                request.Scope.ToDomainScope(),
                [.. request.Permissions],
                request.CreatedBy,
                request.Description,
                request.ExpiresAt?.ToDateTimeOffset()),
            context.CancellationToken);

        return new CreateApiKeyResponse
        {
            ApiKey = result.ApiKey,
            RawKey = result.RawKey,
        };
    }

    public override async Task<GetApiKeyResponse> GetApiKey(GetApiKeyRequest request, ServerCallContext context)
    {
        var apiKey = await mediator.Send(
            new GetApiKeyQuery(Guid.Parse(request.Id)),
            context.CancellationToken);

        return new GetApiKeyResponse { ApiKey = apiKey };
    }

    public override async Task<ListApiKeysResponse> ListApiKeys(ListApiKeysRequest request, ServerCallContext context)
    {
        var scopeFilter = request.ScopeFilter != Contracts.Models.V1.ApiKeyScope.Unspecified
            ? request.ScopeFilter.ToDomainScope()
            : (Domain.Enums.ApiKeyScope?)null;

        var statusFilter = request.StatusFilter switch
        {
            Contracts.Models.V1.ApiKeyStatus.Active => Domain.Enums.ApiKeyStatus.Active,
            Contracts.Models.V1.ApiKeyStatus.Revoked => Domain.Enums.ApiKeyStatus.Revoked,
            Contracts.Models.V1.ApiKeyStatus.Expired => Domain.Enums.ApiKeyStatus.Expired,
            _ => (Domain.Enums.ApiKeyStatus?)null,
        };

        var result = await mediator.Send(
            new ListApiKeysQuery(
                request.Page > 0 ? request.Page : 1,
                request.PageSize > 0 ? request.PageSize : 20,
                scopeFilter,
                statusFilter),
            context.CancellationToken);

        var response = new ListApiKeysResponse { TotalCount = result.TotalCount };
        response.ApiKeys.AddRange(result.ApiKeys);
        return response;
    }

    public override async Task<UpdateApiKeyResponse> UpdateApiKey(UpdateApiKeyRequest request, ServerCallContext context)
    {
        var apiKey = await mediator.Send(
            new UpdateApiKeyCommand(
                Guid.Parse(request.Id),
                request.Name,
                request.Description,
                request.Permissions.Count > 0 ? request.Permissions.ToList() : null,
                request.ExpiresAt?.ToDateTimeOffset()),
            context.CancellationToken);

        return new UpdateApiKeyResponse { ApiKey = apiKey };
    }

    public override async Task<DeleteApiKeyResponse> DeleteApiKey(DeleteApiKeyRequest request, ServerCallContext context)
    {
        var success = await mediator.Send(
            new DeleteApiKeyCommand(Guid.Parse(request.Id)),
            context.CancellationToken);

        return new DeleteApiKeyResponse { Success = success };
    }

    public override async Task<RegenerateApiKeyResponse> RegenerateApiKey(RegenerateApiKeyRequest request, ServerCallContext context)
    {
        var result = await mediator.Send(
            new RegenerateApiKeyCommand(Guid.Parse(request.Id)),
            context.CancellationToken);

        return new RegenerateApiKeyResponse
        {
            ApiKey = result.ApiKey,
            RawKey = result.RawKey,
        };
    }
}
