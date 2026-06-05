using DKH.ApiManagementService.Api.Grpc.Mappers;
using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Contracts.ApiManagement.Api.ModuleState.v1;
using DKH.ApiManagementService.Contracts.ApiManagement.Models.Module.v1;
using DKH.ApiManagementService.Domain.Entities;
using DKH.ApiManagementService.Domain.Enums;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace DKH.ApiManagementService.Api.Grpc.Services;

[Authorize(Policy = ApiManagementServiceAuthorizationPolicies.ApiManagementAdminAccess)]
public class ModuleStateGrpcService(IAppDbContext dbContext, IModuleManifestSource manifestSource)
    : ModuleStateService.ModuleStateServiceBase
{
    public override async Task<ModuleStateModel> InstallModule(InstallModuleRequest request, ServerCallContext context)
    {
        var entity = await dbContext.ModuleStates
            .FirstOrDefaultAsync(x => x.ModuleId == request.ModuleId, context.CancellationToken);

        if (entity is null)
        {
            entity = ModuleStateEntity.Create(request.ModuleId, request.Version, ModuleLifecycleState.Installed);
            dbContext.ModuleStates.Add(entity);
        }
        else
        {
            entity.Install(request.Version);
        }

        await dbContext.SaveChangesAsync(context.CancellationToken);
        return entity.ToStateModel();
    }

    public override Task<ModuleStateModel> EnableModule(EnableModuleRequest request, ServerCallContext context)
        => TransitionAsync(request.ModuleId, entity => entity.Enable(), context);

    public override Task<ModuleStateModel> DisableModule(DisableModuleRequest request, ServerCallContext context)
        => TransitionAsync(request.ModuleId, entity => entity.Disable(), context);

    // Read-only lifecycle state of a deployed module — part of the capabilities manifest every shell
    // reads. Anonymous (overrides the class-level admin gate) so the gateways' per-user proxy call can
    // resolve module state without admin authority. Install/Enable/Disable stay admin-gated.
    [AllowAnonymous]
    public override async Task<ModuleStateModel> GetModuleState(GetModuleStateRequest request, ServerCallContext context)
    {
        var entity = await dbContext.ModuleStates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ModuleId == request.ModuleId, context.CancellationToken);

        if (entity is not null)
        {
            return entity.ToStateModel();
        }

        // No explicit operator record: a module shipped as a manifest is deployed, so it is Enabled by
        // default. An operator Install/Disable writes a real record that overrides this. A module with
        // neither a record nor a manifest is genuinely unknown.
        var components = await manifestSource.GetComponentsAsync(context.CancellationToken);
        var manifest = components.FirstOrDefault(component =>
            string.Equals(component.Id, request.ModuleId, StringComparison.Ordinal));

        return manifest is not null
            ? ModuleStateEntity.Create(manifest.Id, manifest.Version, ModuleLifecycleState.Enabled).ToStateModel()
            : throw new RpcException(new Status(StatusCode.NotFound, $"Module '{request.ModuleId}' has no recorded state."));
    }

    private async Task<ModuleStateModel> TransitionAsync(string moduleId, Action<ModuleStateEntity> apply, ServerCallContext context)
    {
        var entity = await dbContext.ModuleStates
            .FirstOrDefaultAsync(x => x.ModuleId == moduleId, context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Module '{moduleId}' is not installed."));

        apply(entity);
        await dbContext.SaveChangesAsync(context.CancellationToken);
        return entity.ToStateModel();
    }
}
