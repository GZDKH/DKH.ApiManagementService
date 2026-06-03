using DKH.ApiManagementService.Api.Grpc.Mappers;
using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Contracts.ApiManagement.Api.ModuleCatalog.v1;
using DKH.ApiManagementService.Contracts.ApiManagement.Models.Module.v1;
using DKH.Platform.Modularity;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace DKH.ApiManagementService.Api.Grpc.Services;

[Authorize(Policy = ApiManagementServiceAuthorizationPolicies.ApiManagementAdminAccess)]
public class ModuleCatalogGrpcService(IAppDbContext dbContext) : ModuleCatalogService.ModuleCatalogServiceBase
{
    public override async Task<ListComponentsResponse> ListComponents(ListComponentsRequest request, ServerCallContext context)
    {
        var states = await dbContext.ModuleStates.AsNoTracking().ToListAsync(context.CancellationToken);

        var response = new ListComponentsResponse();
        response.Components.AddRange(states.Select(state => state.ToComponentModel()));
        return response;
    }

    // Layer 0: editions are declared via edition.json and ingested in a later phase. None are persisted yet.
    public override Task<ListEditionsResponse> ListEditions(ListEditionsRequest request, ServerCallContext context)
        => Task.FromResult(new ListEditionsResponse());

    public override async Task<ModuleDependencyGraphModel> GetDependencyGraph(GetDependencyGraphRequest request, ServerCallContext context)
    {
        var states = await dbContext.ModuleStates.AsNoTracking().ToListAsync(context.CancellationToken);

        var components = states
            .Select(state => new PlatformModuleManifest
            {
                Id = state.ModuleId,
                Kind = PlatformModuleKind.Service,
                Name = new LocalizedString { ["en"] = state.ModuleId },
                Version = state.Version,
            })
            .ToList();

        var resolution = PlatformModuleDependencyResolver.Resolve(components);

        var model = new ModuleDependencyGraphModel();
        model.OrderedComponentIds.AddRange(resolution.Order);
        model.Problems.AddRange(resolution.Problems.Select(problem => new ModuleResolutionProblem
        {
            Kind = problem.Kind.ToProtoProblemKind(),
            ComponentId = problem.ComponentId,
            Detail = problem.Detail,
        }));

        return model;
    }
}
