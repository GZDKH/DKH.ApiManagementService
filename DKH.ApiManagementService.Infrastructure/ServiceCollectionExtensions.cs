using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Infrastructure.Modularity;
using DKH.ApiManagementService.Infrastructure.Persistence;
using DKH.ApiManagementService.Infrastructure.Persistence.Repositories;
using DKH.ApiManagementService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DKH.ApiManagementService.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiManagementInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        _ = configuration;
        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<DbContext>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
        services.AddScoped<IApiKeyUsageRepository, ApiKeyUsageRepository>();
        services.AddScoped<IAiProviderRepository, AiProviderRepository>();
        services.AddSingleton<IApiKeyGenerator, ApiKeyGenerator>();
        services.AddSingleton<IModuleManifestSource, DirectoryModuleManifestSource>();
        return services;
    }
}
