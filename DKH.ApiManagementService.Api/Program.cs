using DKH.ApiManagementService.Api.Grpc.Services;
using DKH.ApiManagementService.Application;
using DKH.ApiManagementService.Domain.Authorization;
using DKH.ApiManagementService.Domain.Entities;
using DKH.ApiManagementService.Infrastructure;
using DKH.ApiManagementService.Infrastructure.Persistence;
using DKH.Platform;
using DKH.Platform.Authentication.Keycloak;
using DKH.Platform.Authorization;
using DKH.Platform.Authorization.ResourceAccess;
using DKH.Platform.Authorization.ResourceAccess.DependencyInjection;
using DKH.Platform.Authorization.ResourceAccess.Grpc;
using DKH.Platform.Configuration;
using DKH.Platform.Domain.Events;
using DKH.Platform.EntityFrameworkCore.PostgreSQL;
using DKH.Platform.EntityFrameworkCore.Repositories;
using DKH.Platform.Grpc;
using DKH.Platform.Identity;
using DKH.Platform.Logging;
using DKH.Platform.MediatR.Behaviors;
using DKH.Platform.Messaging.MediatR;
using DKH.Platform.Telemetry;

await Platform
    .CreateWeb(args)
    .ConfigurePlatformWebApplicationBuilder(builder =>
    {
        builder.ConfigurePlatformStandardConfiguration();
        builder.Services.AddApiManagementInfrastructure(builder.Configuration);
        builder.Services.AddApplication(builder.Configuration);
    })
    .AddPlatformMessagingWithMediatR(typeof(ConfigureServices).Assembly)
    .AddPlatformMediatRBehaviors()
    .AddPlatformLogging()
    .AddPlatformTelemetry()
    .AddPlatformKeycloakAuth()
    .AddPlatformAuthorization(policies => policies.AddRolePolicy(
        ApiManagementServiceAuthorizationPolicies.ApiManagementAdminAccess,
        PlatformRoles.Realm.SuperAdmin,
        PlatformRoles.Realm.Admin,
        PlatformRoles.FullAccess,
        PlatformRoles.Realm.StorefrontOwner))
    .ConfigurePlatformWebApplicationBuilder(builder =>
        builder.Services.AddPlatformResourceAccess<ApiKeyEntity, ApiManagementAccessGrantEntity, Guid>(opts =>
        {
            opts.ResourceType = "api_key";
            opts.DisplayName = "API Key";
            opts.GrantCreatorFullAccess = true;
            opts.CreatorGrantReason = "creator-default";
            opts.BaselineRoleGrants = b =>
            {
                b.Grant(PlatformRoles.Realm.SuperAdmin,
                        ResourceAccessConstants.WildcardResourceId,
                        ResourceAccessPermissions.FullAccess);
                b.Grant(PlatformRoles.Realm.Admin,
                        ResourceAccessConstants.WildcardResourceId,
                        ResourceAccessPermissions.FullAccess);
            };
        }))
    .AddPlatformPostgreSql<AppDbContext>(options => options.ConnectionStringKey = "Default")
    .AddPlatformRepositories<AppDbContext>()
    .AddPlatformDomainEvents()
    .AddGrpcCurrentUser()
    .AddPlatformGrpc(grpc =>
    {
        grpc.AddInterceptor<ResourceAccessGrpcInterceptor>();
        grpc.MapService<ApiKeyCrudGrpcService>();
        grpc.MapService<ApiKeyValidationGrpcService>();
        grpc.MapService<ApiKeyUsageGrpcService>();
        grpc.MapService<AiProviderCrudGrpcService>();
        grpc.ConfigureDefaultRoute("ApiManagementService gRPC is running.");
    })
    .Build()
    .RunAsync();

internal static class ApiManagementServiceAuthorizationPolicies
{
    public const string ApiManagementAdminAccess = "ApiManagementAdminAccess";
}
