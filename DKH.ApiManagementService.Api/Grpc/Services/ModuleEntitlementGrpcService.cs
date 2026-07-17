using DKH.ApiManagementService.Api.Grpc.Mappers;
using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Contracts.ApiManagement.Api.ModuleEntitlement.v1;
using DKH.Platform.Modularity;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace DKH.ApiManagementService.Api.Grpc.Services;

[Authorize(Policy = ApiManagementServiceAuthorizationPolicies.ApiManagementAdminAccess)]
public class ModuleEntitlementGrpcService(
    IPlatformModuleEntitlementProvider entitlementProvider,
    IAppDbContext dbContext) : ModuleEntitlementService.ModuleEntitlementServiceBase
{
    // Gateways call this read-only check from public routes before user auth has run.
    [AllowAnonymous]
    public override async Task<CheckEntitlementResponse> CheckEntitlement(CheckEntitlementRequest request, ServerCallContext context)
    {
        var granted = await entitlementProvider.IsEntitledAsync(
            request.Scope.ToPlatformScope(),
            request.ModuleId,
            context.CancellationToken);

        return new CheckEntitlementResponse { Granted = granted };
    }

    public override async Task<ListEntitlementsResponse> ListEntitlements(ListEntitlementsRequest request, ServerCallContext context)
    {
        var query = dbContext.ModuleEntitlements.AsNoTracking();

        if (request.Scope is not null)
        {
            var scopeKind = request.Scope.Kind.ToDomainScopeKind();
            var scopeValue = string.IsNullOrEmpty(request.Scope.Value) ? null : request.Scope.Value;
            query = query.Where(x => x.ScopeKind == scopeKind && x.ScopeValue == scopeValue);
        }

        var records = await query.ToListAsync(context.CancellationToken);

        var response = new ListEntitlementsResponse();
        response.Entitlements.AddRange(records.Select(record => record.ToEntitlementModel()));
        return response;
    }
}
