using DKH.ApiManagementService.Api.Grpc.Services;
using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Contracts.ApiManagement.Api.ModuleCatalog.v1;
using DKH.ApiManagementService.Contracts.ApiManagement.Api.ModuleEntitlement.v1;
using DKH.ApiManagementService.Contracts.ApiManagement.Api.ModuleState.v1;
using DKH.ApiManagementService.Infrastructure.Modularity;
using DKH.ApiManagementService.Infrastructure.Persistence;
using DKH.Platform.Authorization;
using DKH.Platform.Domain.Events;
using DKH.Platform.Grpc.IntegrationTesting;
using DKH.Platform.Identity;
using DKH.Platform.IntegrationTesting;
using DKH.Platform.Modularity;
using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using CatalogClient = DKH.ApiManagementService.Contracts.ApiManagement.Api.ModuleCatalog.v1.ModuleCatalogService.ModuleCatalogServiceClient;
using EntitlementClient = DKH.ApiManagementService.Contracts.ApiManagement.Api.ModuleEntitlement.v1.ModuleEntitlementService.ModuleEntitlementServiceClient;
using ProtoModule = DKH.ApiManagementService.Contracts.ApiManagement.Models.Module.v1;
using StateClient = DKH.ApiManagementService.Contracts.ApiManagement.Api.ModuleState.v1.ModuleStateService.ModuleStateServiceClient;

namespace DKH.ApiManagementService.IntegrationTests.Integration.Grpc;

[Trait("Category", "Integration")]
public class ModuleControlPlaneGrpcServiceTests : PlatformIntegrationTest
{
    private enum Caller
    {
        SuperAdmin,
        NonAdmin,
        Anonymous,
    }

    private PlatformGrpcTestFactory<GrpcTestExceptionPolicy> CreateFactory(
        string? manifestsDirectory = null,
        Caller caller = Caller.SuperAdmin,
        params string[] enabledModules)
    {
        var dbName = $"apimgmt-module-{Guid.NewGuid()}";

        var settings = new Dictionary<string, string?>();
        for (var i = 0; i < enabledModules.Length; i++)
        {
            settings[$"{PlatformConfigurationModuleEntitlementProvider.ConfigurationKey}:{i}"] = enabledModules[i];
        }

        if (manifestsDirectory is not null)
        {
            settings["Modularity:ManifestsDirectory"] = manifestsDirectory;
        }

        var factory = this.CreatePlatformGrpcTest<GrpcTestExceptionPolicy>(
                platformBuilder => platformBuilder
                    .AddPlatformAuthorization(policies => policies.AddRolePolicy(
                        "ApiManagementAdminAccess",
                        PlatformRoles.Realm.SuperAdmin)),
                configurationBuilder => configurationBuilder.AddInMemoryCollection(settings),
                typeof(ModuleCatalogGrpcService),
                typeof(ModuleStateGrpcService),
                typeof(ModuleEntitlementGrpcService))
            .WithPlatformConfiguration(services =>
            {
                services.AddSingleton(new Dictionary<Type, object>());

                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
                services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

                services.AddSingleton(Substitute.For<IPlatformDomainEventDispatcher>());
                services.AddSingleton(Substitute.For<IPlatformCurrentUser>());

                services.AddSingleton<IModuleManifestSource, DirectoryModuleManifestSource>();
                services.AddSingleton<IPlatformModuleEntitlementProvider, PlatformConfigurationModuleEntitlementProvider>();
            });

        return caller switch
        {
            Caller.Anonymous => factory.WithAnonymousUser(),
            Caller.NonAdmin => factory.WithAuthenticatedUser(
                userId: Guid.NewGuid(),
                username: "non-admin",
                email: "non-admin@dkh.local",
                roles: [],
                permissions: [],
                tenantId: null,
                additionalClaims: []),
            _ => factory.WithAuthenticatedUser(
                userId: Guid.NewGuid(),
                username: "test-user",
                email: "test@dkh.local",
                roles: [PlatformRoles.Realm.SuperAdmin],
                permissions: [],
                tenantId: null,
                additionalClaims: []),
        };
    }

    [Fact]
    public async Task InstallModule_ThenGetState_ReturnsInstalledAsync()
    {
        await using var factory = CreateFactory();
        var client = this.CreateGrpcClient<StateClient, GrpcTestExceptionPolicy>(factory);

        await client.InstallModuleAsync(new InstallModuleRequest { ModuleId = "dkh.logistics", Version = "1.4.0" });
        var state = await client.GetModuleStateAsync(new GetModuleStateRequest { ModuleId = "dkh.logistics" });

        state.State.Should().Be(ProtoModule.ModuleState.Installed);
        state.Version.Should().Be("1.4.0");
    }

    [Fact]
    public async Task EnableModule_TransitionsStateToEnabledAsync()
    {
        await using var factory = CreateFactory();
        var client = this.CreateGrpcClient<StateClient, GrpcTestExceptionPolicy>(factory);

        await client.InstallModuleAsync(new InstallModuleRequest { ModuleId = "dkh.logistics", Version = "1.4.0" });
        await client.EnableModuleAsync(new EnableModuleRequest { ModuleId = "dkh.logistics" });
        var state = await client.GetModuleStateAsync(new GetModuleStateRequest { ModuleId = "dkh.logistics" });

        state.State.Should().Be(ProtoModule.ModuleState.Enabled);
    }

    [Fact]
    public async Task CheckEntitlement_ReflectsConfiguredEnabledModulesAsync()
    {
        await using var factory = CreateFactory(manifestsDirectory: null, enabledModules: "dkh.logistics");
        var client = this.CreateGrpcClient<EntitlementClient, GrpcTestExceptionPolicy>(factory);

        var granted = await client.CheckEntitlementAsync(new CheckEntitlementRequest { ModuleId = "dkh.logistics" });
        var denied = await client.CheckEntitlementAsync(new CheckEntitlementRequest { ModuleId = "dkh.payments" });

        granted.Granted.Should().BeTrue();
        denied.Granted.Should().BeFalse();
    }

    [Fact]
    public async Task ListComponents_IngestsManifestsFromDirectoryAsync()
    {
        var directory = Directory.CreateTempSubdirectory("dkh-modules-").FullName;
        await File.WriteAllTextAsync(
            Path.Combine(directory, "module.json"),
                                 /*lang=json,strict*/
                                 """{ "id": "dkh.logistics", "kind": "service", "name": { "en": "Logistics" }, "version": "1.4.0", "requiresEntitlement": "logistics" }""");

        await using var factory = CreateFactory(manifestsDirectory: directory);
        var client = this.CreateGrpcClient<CatalogClient, GrpcTestExceptionPolicy>(factory);

        var response = await client.ListComponentsAsync(new ListComponentsRequest());

        var component = response.Components.Should().ContainSingle().Subject;
        component.Id.Should().Be("dkh.logistics");
        component.Kind.Should().Be(ProtoModule.ModuleKind.Service);
        component.RequiresEntitlement.Should().Be("logistics");
        component.State.Should().Be(ProtoModule.ModuleState.Discovered);
    }

    [Fact]
    public async Task ListCatalog_AllowsAnonymousCaller_ForCapabilitiesManifestAsync()
    {
        var directory = Directory.CreateTempSubdirectory("dkh-modules-").FullName;
        await File.WriteAllTextAsync(
            Path.Combine(directory, "module.json"),
                                 /*lang=json,strict*/
                                 """{ "id": "dkh.logistics", "kind": "service", "name": { "en": "Logistics" }, "version": "1.4.0", "requiresEntitlement": "logistics" }""");

        await using var factory = CreateFactory(manifestsDirectory: directory, caller: Caller.Anonymous);
        var client = this.CreateGrpcClient<CatalogClient, GrpcTestExceptionPolicy>(factory);

        // The gateways proxy these reads without admin authority — they must resolve anonymously
        // so the capabilities endpoint can never 403.
        var components = await client.ListComponentsAsync(new ListComponentsRequest());
        var editions = await client.ListEditionsAsync(new ListEditionsRequest());

        components.Components.Should().ContainSingle(component => component.Id == "dkh.logistics");
        editions.Editions.Should().BeEmpty();
    }

    [Fact]
    public async Task GetModuleState_AllowsAnonymousCaller_ReachesHandlerAsync()
    {
        await using var factory = CreateFactory(caller: Caller.Anonymous);
        var client = this.CreateGrpcClient<StateClient, GrpcTestExceptionPolicy>(factory);

        var act = async () => await client.GetModuleStateAsync(new GetModuleStateRequest { ModuleId = "dkh.unknown" });

        // NotFound (not PermissionDenied / Unauthenticated) proves the anonymous call reached the
        // handler — the module simply has no recorded state.
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Fact]
    public async Task InstallModule_StillRequiresAdmin_RejectsNonAdminCallerAsync()
    {
        await using var factory = CreateFactory(caller: Caller.NonAdmin);
        var client = this.CreateGrpcClient<StateClient, GrpcTestExceptionPolicy>(factory);

        var act = async () => await client.InstallModuleAsync(
            new InstallModuleRequest { ModuleId = "dkh.logistics", Version = "1.4.0" });

        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.PermissionDenied);
    }

    [Fact]
    public async Task ListEntitlements_StillRequiresAdmin_RejectsNonAdminCallerAsync()
    {
        await using var factory = CreateFactory(caller: Caller.NonAdmin);
        var client = this.CreateGrpcClient<EntitlementClient, GrpcTestExceptionPolicy>(factory);

        var act = async () => await client.ListEntitlementsAsync(new ListEntitlementsRequest());

        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.PermissionDenied);
    }
}
