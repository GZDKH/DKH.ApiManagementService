using DKH.ApiManagementService.Application.Features.AiProviders.Commands.CreateAiProvider;
using DKH.ApiManagementService.Application.Features.AiProviders.Commands.DeleteAiProvider;
using DKH.ApiManagementService.Application.Features.AiProviders.Commands.UpdateAiProvider;
using DKH.ApiManagementService.Application.Features.AiProviders.Mappers;
using DKH.ApiManagementService.Application.Features.AiProviders.Queries.GetAiProvider;
using DKH.ApiManagementService.Application.Features.AiProviders.Queries.ListAiProviders;
using DKH.ApiManagementService.Contracts.ApiManagement.Api.AiProviderCrud.v1;
using DKH.ApiManagementService.Contracts.ApiManagement.Models.AiProvider.v1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;

namespace DKH.ApiManagementService.Api.Grpc.Services;

public class AiProviderCrudGrpcService(IMediator mediator) : AiProviderCrudService.AiProviderCrudServiceBase
{
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

    public override async Task<AiProviderModel> Get(GetAiProviderRequest request, ServerCallContext context)
    {
        return await mediator.Send(
            new GetAiProviderQuery(request.Id),
            context.CancellationToken);
    }

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

        var result = await mediator.Send(
            new ListAiProvidersQuery(
                request.Pagination?.Page > 0 ? request.Pagination.Page : 1,
                request.Pagination?.PageSize > 0 ? request.Pagination.PageSize : 20,
                typeFilter,
                statusFilter),
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

    public override async Task<Empty> Delete(DeleteAiProviderRequest request, ServerCallContext context)
    {
        await mediator.Send(
            new DeleteAiProviderCommand(request.Id),
            context.CancellationToken);

        return new Empty();
    }
}
