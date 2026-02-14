using DKH.ApiManagementService.Api.Grpc.Services;
using DKH.ApiManagementService.Application;
using DKH.ApiManagementService.Infrastructure;
using DKH.ApiManagementService.Infrastructure.Persistence;
using DKH.Platform;
using DKH.Platform.Configuration;
using DKH.Platform.Domain.Events;
using DKH.Platform.EntityFrameworkCore.PostgreSQL;
using DKH.Platform.EntityFrameworkCore.Repositories;
using DKH.Platform.Grpc;
using DKH.Platform.Identity;
using DKH.Platform.Logging;
using DKH.Platform.MediatR.Behaviors;
using DKH.Platform.Messaging.MediatR;

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
    .AddPlatformPostgreSql<AppDbContext>(options => options.ConnectionStringKey = "Default")
    .AddPlatformRepositories<AppDbContext>()
    .AddPlatformDomainEvents()
    .AddGrpcCurrentUser()
    .AddPlatformGrpc(grpc =>
    {
        grpc.ConfigureServer(options => options.EnableDetailedErrors = true);
        grpc.MapService<ApiKeyCrudGrpcService>();
        grpc.MapService<ApiKeyValidationGrpcService>();
        grpc.MapService<ApiKeyUsageGrpcService>();
        grpc.ConfigureDefaultRoute("ApiManagementService gRPC is running.");
    })
    .Build()
    .RunAsync();
