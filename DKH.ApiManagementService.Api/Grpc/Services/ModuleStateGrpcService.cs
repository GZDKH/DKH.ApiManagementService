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
public class ModuleStateGrpcService(IAppDbContext dbContext) : ModuleStateService.ModuleStateServiceBase
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

    public override async Task<ModuleStateModel> GetModuleState(GetModuleStateRequest request, ServerCallContext context)
    {
        var entity = await dbContext.ModuleStates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ModuleId == request.ModuleId, context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Module '{request.ModuleId}' has no recorded state."));

        return entity.ToStateModel();
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
