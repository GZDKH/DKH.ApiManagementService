using DKH.ApiManagementService.Application.Features.Usage.RecordUsage;
using DKH.ApiManagementService.Domain.Entities;
using DKH.ApiManagementService.Domain.Enums;
using DKH.ApiManagementService.Domain.ValueObjects;
using DKH.ApiManagementService.Infrastructure.Persistence;
using DKH.ApiManagementService.Infrastructure.Persistence.Repositories;
using DKH.Platform.Identity;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace DKH.ApiManagementService.Tests.Usage;

public sealed class RecordUsageCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenApiKeyExists_SnapshotsPublicApiAnalyticsDimensionsAsync()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Substitute.For<IPlatformCurrentUser>());
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase($"api-key-usage-{Guid.NewGuid()}"));

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var customerId = Guid.NewGuid();
        var apiKey = ApiKeyEntity.Create(
            "partner-public-key",
            ApiKeyHash.FromRawKey("dkh_ptr_usage_key_aaaaaaaaaaaaaaaa").Value,
            "dkh_ptr_usage_",
            ApiKeyScope.Partner,
            ["orders.read"],
            customerId,
            ApiKeyEnvironment.Production,
            ApiKeyRateLimitTier.Enterprise);

        dbContext.ApiKeys.Add(apiKey);
        await dbContext.SaveChangesAsync();

        var handler = new RecordUsageCommandHandler(new ApiKeyUsageRepository(dbContext), dbContext);

        await handler.Handle(
            new RecordUsageCommand(apiKey.Id, "/public/orders", 200, "127.0.0.1", "integration-test", 42),
            CancellationToken.None);

        var usage = await dbContext.ApiKeyUsageRecords.SingleAsync();
        usage.CustomerId.Should().Be(customerId);
        usage.Environment.Should().Be(ApiKeyEnvironment.Production);
        usage.RateLimitTier.Should().Be(ApiKeyRateLimitTier.Enterprise);
        usage.RateLimitRequestsPerMinute.Should().Be(12_000);
    }
}
