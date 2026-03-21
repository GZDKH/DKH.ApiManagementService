using DKH.ApiManagementService.Application.Features.ApiKeys.Commands.CreateApiKey;
using DKH.ApiManagementService.Application.Features.ApiKeys.Commands.DeleteApiKey;
using DKH.ApiManagementService.Application.Features.ApiKeys.Commands.PermanentlyDeleteApiKey;
using DKH.ApiManagementService.Application.Features.ApiKeys.Commands.RegenerateApiKey;
using DKH.ApiManagementService.Application.Features.ApiKeys.Commands.RestoreApiKey;
using DKH.ApiManagementService.Application.Features.ApiKeys.Commands.UpdateApiKey;
using DKH.ApiManagementService.Application.Features.ApiKeys.Mappers;
using DKH.ApiManagementService.Application.Features.ApiKeys.Queries.GetApiKey;
using DKH.ApiManagementService.Application.Features.ApiKeys.Queries.ListApiKeys;
using DKH.ApiManagementService.Contracts.ApiManagement.Api.ApiKeyCrud.v1;
using DKH.ApiManagementService.Contracts.ApiManagement.Models.ApiKey.v1;
using DKH.Platform.Grpc.Extensions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;

namespace DKH.ApiManagementService.Api.Grpc.Services;

public class ApiKeyCrudGrpcService(IMediator mediator) : ApiKeysCrudService.ApiKeysCrudServiceBase
{
    public override async Task<CreateApiKeyResponse> CreateApiKey(CreateApiKeyRequest request, ServerCallContext context)
    {
        var result = await mediator.Send(
            new CreateApiKeyCommand(
                request.Name,
                request.Scope.ToDomainScope(),
                [.. request.Permissions],
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
            new GetApiKeyQuery(request.Id),
            context.CancellationToken);

        return new GetApiKeyResponse { ApiKey = apiKey };
    }

    public override async Task<ListApiKeysResponse> ListApiKeys(ListApiKeysRequest request, ServerCallContext context)
    {
        var scopeFilter = request.ScopeFilter != ApiKeyScope.Unspecified
            ? request.ScopeFilter.ToDomainScope()
            : (Domain.Enums.ApiKeyScope?)null;

        var statusFilter = request.StatusFilter switch
        {
            ApiKeyStatus.Active => Domain.Enums.ApiKeyStatus.Active,
            ApiKeyStatus.Revoked => Domain.Enums.ApiKeyStatus.Revoked,
            ApiKeyStatus.Expired => Domain.Enums.ApiKeyStatus.Expired,
            _ => (Domain.Enums.ApiKeyStatus?)null,
        };

        var softDeleteFilter = request.HasSoftDeleteFilter
            ? request.SoftDeleteFilter.ToDomain()
            : Platform.Domain.Enums.PlatformSoftDeleteFilter.ActiveOnly;

        var result = await mediator.Send(
            new ListApiKeysQuery(
                request.Pagination?.Page > 0 ? request.Pagination.Page : 1,
                request.Pagination?.PageSize > 0 ? request.Pagination.PageSize : 20,
                scopeFilter,
                statusFilter,
                softDeleteFilter),
            context.CancellationToken);

        var page = request.Pagination?.Page ?? 1;
        var pageSize = request.Pagination?.PageSize ?? 20;
        var totalPages = (int)Math.Ceiling((double)result.TotalCount / pageSize);

        var response = new ListApiKeysResponse
        {
            Metadata = new Platform.Grpc.Common.Types.PaginationMetadata
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = result.TotalCount,
                TotalPages = totalPages,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1,
            },
        };
        response.ApiKeys.AddRange(result.ApiKeys);
        return response;
    }

    public override async Task<UpdateApiKeyResponse> UpdateApiKey(UpdateApiKeyRequest request, ServerCallContext context)
    {
        var apiKey = await mediator.Send(
            new UpdateApiKeyCommand(
                request.Id,
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
            new DeleteApiKeyCommand(request.Id),
            context.CancellationToken);

        return new DeleteApiKeyResponse { Success = success };
    }

    public override async Task<RegenerateApiKeyResponse> RegenerateApiKey(RegenerateApiKeyRequest request, ServerCallContext context)
    {
        var result = await mediator.Send(
            new RegenerateApiKeyCommand(request.Id),
            context.CancellationToken);

        return new RegenerateApiKeyResponse
        {
            ApiKey = result.ApiKey,
            RawKey = result.RawKey,
        };
    }

    public override async Task<ApiKeyModel> RestoreApiKey(RestoreApiKeyRequest request, ServerCallContext context)
    {
        return await mediator.Send(
            new RestoreApiKeyCommand(request.Id),
            context.CancellationToken);
    }

    public override async Task<Empty> PermanentlyDeleteApiKey(PermanentlyDeleteApiKeyRequest request, ServerCallContext context)
    {
        await mediator.Send(
            new PermanentlyDeleteApiKeyCommand(request.Id),
            context.CancellationToken);

        return new Empty();
    }
}
