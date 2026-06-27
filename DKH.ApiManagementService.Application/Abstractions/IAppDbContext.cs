using DKH.ApiManagementService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DKH.ApiManagementService.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<ApiKeyEntity> ApiKeys { get; }

    DbSet<ApiKeyUsageEntity> ApiKeyUsageRecords { get; }

    DbSet<AiProviderEntity> AiProviders { get; }

    DbSet<ModuleStateEntity> ModuleStates { get; }

    DbSet<ModuleEntitlementEntity> ModuleEntitlements { get; }

    DbSet<ReportedModuleComponentEntity> ReportedModuleComponents { get; }

    DbSet<WebhookSubscriptionEntity> WebhookSubscriptions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
