using DKH.ApiManagementService.Api.Auth;
using DKH.ApiManagementService.Api.Grpc.Services;
using DKH.ApiManagementService.Application;
using DKH.ApiManagementService.Domain.Authorization;
using DKH.ApiManagementService.Domain.Entities;
using DKH.ApiManagementService.Infrastructure;
using DKH.ApiManagementService.Infrastructure.Persistence;
using DKH.Platform;
using DKH.Platform.ApiKeyAuth;
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
using DKH.Platform.Modularity;
using DKH.Platform.RestfulApi;
using DKH.Platform.Telemetry;
using Microsoft.Extensions.Options;

await Platform
    .CreateWeb(args)
    .ConfigurePlatformWebApplicationBuilder(builder =>
    {
        builder.ConfigurePlatformStandardConfiguration();
        builder.Services.AddApiManagementInfrastructure(builder.Configuration);
        builder.Services.AddApplication(builder.Configuration);
        builder.Services.AddSingleton<IPlatformModuleEntitlementProvider, PlatformConfigurationModuleEntitlementProvider>();
    })
    .AddPlatformMessagingWithMediatR(typeof(ConfigureServices).Assembly)
    .AddPlatformMediatRBehaviors()
    .AddPlatformLogging()
    .AddPlatformTelemetry()
    .AddPlatformKeycloakAuth()
    .AddPlatformApiKeyAuth<ApiKeyValidator>()
    .ConfigurePlatformWebApplicationBuilder(builder =>
        builder.Services
            .AddOptions<PlatformApiKeyAuthOptions>()
            .Bind(builder.Configuration.GetSection(PlatformApiKeyAuthOptions.Section)))
    .ConfigurePlatformWebApplication(app =>
    {
        // API key auth is an ADDITIONAL scheme alongside Keycloak — only engage the middleware
        // when the configured header is actually presented. Requests without the header fall
        // through to the rest of the pipeline (Keycloak JWT for gRPC callers, health probes, etc.).
        var apiKeyOptions = app.Services
            .GetRequiredService<IOptions<PlatformApiKeyAuthOptions>>()
            .Value;
        app.UseWhen(
            ctx => ctx.Request.Headers.ContainsKey(apiKeyOptions.HeaderName),
            branch => branch.UseMiddleware<PlatformApiKeyAuthMiddleware>());
    })
    .AddPlatformAuthorization(policies => policies.AddRolePolicy(
        ApiManagementServiceAuthorizationPolicies.ApiManagementAdminAccess,
        PlatformRoles.Realm.SuperAdmin,
        PlatformRoles.Realm.Admin,
        PlatformRoles.FullAccess,
        PlatformRoles.Realm.StorefrontOwner)
        .AddRolePolicy(
            ApiManagementServiceAuthorizationPolicies.ScopeTokenIssuerAccess,
            PlatformRoles.Realm.SuperAdmin,
            PlatformRoles.Realm.Admin,
            PlatformRoles.FullAccess,
            PlatformRoles.Realm.StorefrontOwner,
            "engagement.operator"))
    .ConfigurePlatformWebApplicationBuilder(builder =>
    {
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
        });
        builder.Services.AddPlatformResourceAccess<AiProviderEntity, ApiManagementAccessGrantEntity, Guid>(opts =>
        {
            opts.ResourceType = "ai_provider";
            opts.DisplayName = "AI Provider";
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
        });
    })
    .AddPlatformRestfulApi(api => api.ConfigureConfiguration())
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
        grpc.MapService<ScopeTokenGrpcService>();
        grpc.MapService<AiProviderCrudGrpcService>();
        grpc.MapService<ApiManagementGrantsGrpcService>();
        grpc.MapService<ModuleCatalogGrpcService>();
        grpc.MapService<ModuleStateGrpcService>();
        grpc.MapService<ModuleEntitlementGrpcService>();
        grpc.ConfigureDefaultRoute("ApiManagementService gRPC is running.");
    })
    .Build()
    .RunAsync();

internal static class ApiManagementServiceAuthorizationPolicies
{
    public const string ApiManagementAdminAccess = "ApiManagementAdminAccess";
    public const string ScopeTokenIssuerAccess = "ScopeTokenIssuerAccess";
}
