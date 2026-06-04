using DKH.ApiManagementService.Api.Grpc.Mappers;
using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Contracts.ApiManagement.Api.ModuleCatalog.v1;
using DKH.ApiManagementService.Contracts.ApiManagement.Models.Module.v1;
using DKH.Platform.Modularity;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace DKH.ApiManagementService.Api.Grpc.Services;

// Read-only deployed-module catalog (components, editions, dependency graph) — the manifest every
// admin/storefront shell reads to render its navigation, not admin-sensitive data. The gateways
// proxy this per-user call without admin authority (Authorization is stripped by design), so the
// reads are anonymous and the capabilities endpoint can never 403. State transitions and
// entitlement grants stay admin-gated on their own services.
[AllowAnonymous]
public class ModuleCatalogGrpcService(IModuleManifestSource manifestSource, IAppDbContext dbContext)
    : ModuleCatalogService.ModuleCatalogServiceBase
{
    public override async Task<ListComponentsResponse> ListComponents(ListComponentsRequest request, ServerCallContext context)
    {
        var components = await manifestSource.GetComponentsAsync(context.CancellationToken);
        var stateByModuleId = await dbContext.ModuleStates
            .AsNoTracking()
            .ToDictionaryAsync(x => x.ModuleId, x => x.State, context.CancellationToken);

        var response = new ListComponentsResponse();
        response.Components.AddRange(components.Select(component =>
            component.ToComponentModel(stateByModuleId.TryGetValue(component.Id, out var state) ? state : null)));
        return response;
    }

    public override async Task<ListEditionsResponse> ListEditions(ListEditionsRequest request, ServerCallContext context)
    {
        var editions = await manifestSource.GetEditionsAsync(context.CancellationToken);

        var response = new ListEditionsResponse();
        response.Editions.AddRange(editions.Select(edition => edition.ToEditionModel()));
        return response;
    }

    public override async Task<ModuleDependencyGraphModel> GetDependencyGraph(GetDependencyGraphRequest request, ServerCallContext context)
    {
        var components = await manifestSource.GetComponentsAsync(context.CancellationToken);
        var editions = await manifestSource.GetEditionsAsync(context.CancellationToken);

        var resolution = PlatformModuleDependencyResolver.Resolve(components, editions);

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
