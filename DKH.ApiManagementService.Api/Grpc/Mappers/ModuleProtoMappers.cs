using DKH.ApiManagementService.Domain.Entities;
using DKH.ApiManagementService.Domain.Enums;
using DKH.Platform.Modularity;
using ProtoModule = DKH.ApiManagementService.Contracts.ApiManagement.Models.Module.v1;

namespace DKH.ApiManagementService.Api.Grpc.Mappers;

internal static class ModuleProtoMappers
{
    public static ProtoModule.ModuleState ToProto(this ModuleLifecycleState state) => state switch
    {
        ModuleLifecycleState.Discovered => ProtoModule.ModuleState.Discovered,
        ModuleLifecycleState.Installed => ProtoModule.ModuleState.Installed,
        ModuleLifecycleState.Enabled => ProtoModule.ModuleState.Enabled,
        ModuleLifecycleState.Disabled => ProtoModule.ModuleState.Disabled,
        ModuleLifecycleState.Failed => ProtoModule.ModuleState.Failed,
        _ => ProtoModule.ModuleState.Unspecified,
    };

    public static ModuleEntitlementScopeKind ToDomainScopeKind(this ProtoModule.ModuleScopeKind kind) => kind switch
    {
        ProtoModule.ModuleScopeKind.Tenant => ModuleEntitlementScopeKind.Tenant,
        ProtoModule.ModuleScopeKind.Storefront => ModuleEntitlementScopeKind.Storefront,
        ProtoModule.ModuleScopeKind.License => ModuleEntitlementScopeKind.License,
        _ => ModuleEntitlementScopeKind.Deployment,
    };

    public static ProtoModule.ModuleScopeKind ToProto(this ModuleEntitlementScopeKind kind) => kind switch
    {
        ModuleEntitlementScopeKind.Tenant => ProtoModule.ModuleScopeKind.Tenant,
        ModuleEntitlementScopeKind.Storefront => ProtoModule.ModuleScopeKind.Storefront,
        ModuleEntitlementScopeKind.License => ProtoModule.ModuleScopeKind.License,
        _ => ProtoModule.ModuleScopeKind.Deployment,
    };

    public static PlatformModuleScope ToPlatformScope(this ProtoModule.ModuleScope? scope)
    {
        if (scope is null)
        {
            return PlatformModuleScope.Deployment;
        }

        return scope.Kind switch
        {
            ProtoModule.ModuleScopeKind.Tenant => PlatformModuleScope.ForTenant(scope.Value),
            ProtoModule.ModuleScopeKind.Storefront => PlatformModuleScope.ForStorefront(scope.Value),
            ProtoModule.ModuleScopeKind.License => PlatformModuleScope.ForLicense(scope.Value),
            _ => PlatformModuleScope.Deployment,
        };
    }

    public static ProtoModule.ModuleResolutionProblemKind ToProtoProblemKind(this PlatformModuleResolutionProblemKind kind) => kind switch
    {
        PlatformModuleResolutionProblemKind.MissingComponent => ProtoModule.ModuleResolutionProblemKind.MissingComponent,
        PlatformModuleResolutionProblemKind.MissingCapability => ProtoModule.ModuleResolutionProblemKind.MissingCapability,
        PlatformModuleResolutionProblemKind.VersionConflict => ProtoModule.ModuleResolutionProblemKind.VersionConflict,
        PlatformModuleResolutionProblemKind.Cycle => ProtoModule.ModuleResolutionProblemKind.Cycle,
        PlatformModuleResolutionProblemKind.Unentitled => ProtoModule.ModuleResolutionProblemKind.Unentitled,
        _ => ProtoModule.ModuleResolutionProblemKind.Unspecified,
    };

    public static ProtoModule.ModuleStateModel ToStateModel(this ModuleStateEntity entity) => new()
    {
        ModuleId = entity.ModuleId,
        State = entity.State.ToProto(),
        Version = entity.Version,
    };

    public static ProtoModule.ModuleComponentModel ToComponentModel(this ModuleStateEntity entity) => new()
    {
        Id = entity.ModuleId,
        Kind = ProtoModule.ModuleKind.Service,
        Version = entity.Version,
        State = entity.State.ToProto(),
        RequiresEntitlement = string.Empty,
    };

    public static ProtoModule.ModuleEntitlementModel ToEntitlementModel(this ModuleEntitlementEntity entity) => new()
    {
        Scope = new ProtoModule.ModuleScope { Kind = entity.ScopeKind.ToProto(), Value = entity.ScopeValue ?? string.Empty },
        ModuleId = entity.ModuleId,
        Granted = entity.Granted,
    };
}
