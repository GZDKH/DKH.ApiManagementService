using DKH.ApiManagementService.Application.Features.AiProviders.Commands.CreateAiProvider;
using DKH.ApiManagementService.Application.Features.AiProviders.Commands.DeleteAiProvider;
using DKH.ApiManagementService.Application.Features.AiProviders.Commands.PermanentlyDeleteAiProvider;
using DKH.ApiManagementService.Application.Features.AiProviders.Commands.RestoreAiProvider;
using DKH.ApiManagementService.Application.Features.AiProviders.Commands.UpdateAiProvider;
using DKH.ApiManagementService.Application.Features.AiProviders.Mappers;
using DKH.ApiManagementService.Application.Features.AiProviders.Queries.GetAiProvider;
using DKH.ApiManagementService.Application.Features.AiProviders.Queries.ListAiProviders;
using DKH.ApiManagementService.Contracts.ApiManagement.Api.AiProviderCrud.v1;
using DKH.ApiManagementService.Contracts.ApiManagement.Models.AiProvider.v1;
using DKH.Platform.Authorization.ResourceAccess;
using DKH.Platform.Authorization.ResourceAccess.Attributes;
using DKH.Platform.Domain.Enums;
using DKH.Platform.Grpc.Extensions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace DKH.ApiManagementService.Api.Grpc.Services;

[Authorize(Policy = ApiManagementServiceAuthorizationPolicies.ApiManagementAdminAccess)]
public class AiProviderCrudGrpcService(IMediator mediator) : AiProviderCrudService.AiProviderCrudServiceBase
{
    // Create is admin-only via the AdminAccess policy. The resulting AiProvider gets a creator
    // grant automatically (GrantCreatorFullAccess) and downstream operations rely on it.
    public override async Task<AiProviderModel> Create(CreateAiProviderRequest request, ServerCallContext context)
    {
        return await mediator.Send(
            new CreateAiProviderCommand(
                request.Name,
                request.ProviderType.ToDomainType(),
                request.DisplayName,
                request.BaseUrl,
                request.Models.Count > 0 ? [.. request.Models] : null,
                request.ApiKeyReference,
                request.RateLimitPerMinute,
                request.DailyQuota),
            context.CancellationToken);
    }

    [RequireResourceAccess("ai_provider", ResourceAccessPermissions.Read, ResourceIdProperty = "Id")]
    public override async Task<AiProviderModel> Get(GetAiProviderRequest request, ServerCallContext context)
    {
        return await mediator.Send(
            new GetAiProviderQuery(request.Id),
            context.CancellationToken);
    }

    // List is admin-only; SuperAdmin/Admin baseline grants on ai_provider cover the read path.
    // A handler-level ApplyResourceAccessFilter can be added later for non-admin scopes.
    public override async Task<ListAiProvidersResponse> List(ListAiProvidersRequest request, ServerCallContext context)
    {
        var typeFilter = request.TypeFilter != AiProviderType.Unspecified
            ? request.TypeFilter.ToDomainType()
            : (Domain.Enums.AiProviderType?)null;

        var statusFilter = request.StatusFilter switch
        {
            AiProviderStatus.Active => Domain.Enums.AiProviderStatus.Active,
            AiProviderStatus.Inactive => Domain.Enums.AiProviderStatus.Inactive,
            AiProviderStatus.Error => Domain.Enums.AiProviderStatus.Error,
            _ => (Domain.Enums.AiProviderStatus?)null,
        };

        var softDeleteFilter = request.HasSoftDeleteFilter
            ? request.SoftDeleteFilter.ToDomain()
            : PlatformSoftDeleteFilter.ActiveOnly;

        var result = await mediator.Send(
            new ListAiProvidersQuery(
                request.Pagination?.Page > 0 ? request.Pagination.Page : 1,
                request.Pagination?.PageSize > 0 ? request.Pagination.PageSize : 20,
                typeFilter,
                statusFilter,
                softDeleteFilter),
            context.CancellationToken);

        var page = request.Pagination?.Page ?? 1;
        var pageSize = request.Pagination?.PageSize ?? 20;
        var totalPages = (int)Math.Ceiling((double)result.TotalCount / pageSize);

        var response = new ListAiProvidersResponse
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
        response.Providers.AddRange(result.Providers);
        return response;
    }

    [RequireResourceAccess("ai_provider", ResourceAccessPermissions.Update, ResourceIdProperty = "Id")]
    public override async Task<AiProviderModel> Update(UpdateAiProviderRequest request, ServerCallContext context)
    {
        return await mediator.Send(
            new UpdateAiProviderCommand(
                request.Id,
                request.Name,
                request.DisplayName,
                request.BaseUrl,
                request.Models.Count > 0 ? [.. request.Models] : null,
                request.ApiKeyReference,
                request.RateLimitPerMinute,
                request.DailyQuota),
            context.CancellationToken);
    }

    [RequireResourceAccess("ai_provider", ResourceAccessPermissions.Delete, ResourceIdProperty = "Id")]
    public override async Task<Empty> Delete(DeleteAiProviderRequest request, ServerCallContext context)
    {
        await mediator.Send(
            new DeleteAiProviderCommand(request.Id),
            context.CancellationToken);

        return new Empty();
    }

    [RequireResourceAccess("ai_provider", ResourceAccessPermissions.Update, ResourceIdProperty = "Id")]
    public override async Task<AiProviderModel> Restore(RestoreAiProviderRequest request, ServerCallContext context)
    {
        return await mediator.Send(
            new RestoreAiProviderCommand(request.Id),
            context.CancellationToken);
    }

    [RequireResourceAccess("ai_provider", ResourceAccessPermissions.Delete, ResourceIdProperty = "Id")]
    public override async Task<Empty> PermanentlyDelete(PermanentlyDeleteAiProviderRequest request, ServerCallContext context)
    {
        await mediator.Send(
            new PermanentlyDeleteAiProviderCommand(request.Id),
            context.CancellationToken);

        return new Empty();
    }

    // Internal service-to-service AI-key resolution for AI runtimes (AssistantService) that resolve
    // the active provider while serving a merchant request. Those callers carry the merchant's token
    // (StorefrontsWrite), which by design cannot satisfy ApiManagementAdminAccess — gating this read
    // behind the admin policy is what left runtime AI-key resolution returning 401. Anonymous like
    // ModuleStateGrpcService.GetModuleState — an internal call on the docker network, and the network
    // boundary is the control. Least privilege: one active provider, runtime fields only, never the
    // admin listing surface.
    [AllowAnonymous]
    public override async Task<ResolveActiveProviderResponse> ResolveActiveProvider(
        ResolveActiveProviderRequest request,
        ServerCallContext context)
    {
        if (request.ProviderType == AiProviderType.Unspecified)
        {
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, "ResolveActiveProvider requires a provider_type."));
        }

        var result = await mediator.Send(
            new ListAiProvidersQuery(
                1,
                1,
                request.ProviderType.ToDomainType(),
                Domain.Enums.AiProviderStatus.Active,
                PlatformSoftDeleteFilter.ActiveOnly),
            context.CancellationToken);

        var response = new ResolveActiveProviderResponse();
        if (result.Providers.Count == 0)
        {
            return response;
        }

        var provider = result.Providers[0];
        var resolved = new ResolvedAiProvider { ProviderType = provider.ProviderType };

        if (provider.BaseUrl is not null)
        {
            resolved.BaseUrl = provider.BaseUrl;
        }

        if (provider.ApiKeyReference is not null)
        {
            resolved.ApiKeyReference = provider.ApiKeyReference;
        }

        resolved.Models.AddRange(provider.Models);
        response.Provider = resolved;
        return response;
    }
}
