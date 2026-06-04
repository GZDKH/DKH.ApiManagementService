using DKH.ApiManagementService.Domain.Entities;
using DKH.ApiManagementService.Domain.Enums;
using DKH.ApiManagementService.Domain.ValueObjects;
using FluentAssertions;

namespace DKH.ApiManagementService.Tests.ApiKeys;

public sealed class ApiKeyPublicApiHardeningTests
{
    [Fact]
    public void Create_WithPublicApiMetadata_CapturesCustomerEnvironmentTierAndLimit()
    {
        var customerId = Guid.NewGuid();

        var entity = CreatePublicApiKey(customerId, ApiKeyEnvironment.Production, ApiKeyRateLimitTier.Professional);

        entity.CustomerId.Should().Be(customerId);
        entity.Environment.Should().Be(ApiKeyEnvironment.Production);
        entity.RateLimitTier.Should().Be(ApiKeyRateLimitTier.Professional);
        entity.RateLimitRequestsPerMinute.Should().Be(3_000);
    }

    [Fact]
    public void Regenerate_TracksRotationWithoutChangingCustomerPolicy()
    {
        var customerId = Guid.NewGuid();
        var entity = CreatePublicApiKey(customerId, ApiKeyEnvironment.Sandbox, ApiKeyRateLimitTier.Standard);

        entity.Regenerate(ApiKeyHash.FromRawKey("dkh_ptr_new_key_aaaaaaaaaaaaaaaa").Value, "dkh_ptr_new_");

        entity.LastRotatedAt.Should().NotBeNull();
        entity.RotationCount.Should().Be(1);
        entity.PreviousKeyPrefix.Should().Be("dkh_ptr_old_");
        entity.CustomerId.Should().Be(customerId);
        entity.Environment.Should().Be(ApiKeyEnvironment.Sandbox);
        entity.RateLimitTier.Should().Be(ApiKeyRateLimitTier.Standard);
        entity.RateLimitRequestsPerMinute.Should().Be(120);
    }

    [Fact]
    public void UsageRecord_CapturesAnalyticsDimensionsAtRecordTime()
    {
        var customerId = Guid.NewGuid();
        var apiKeyId = Guid.NewGuid();

        var usage = ApiKeyUsageEntity.Create(
            apiKeyId,
            "/public/orders",
            200,
            "127.0.0.1",
            "integration-test",
            42L,
            customerId,
            ApiKeyEnvironment.Production,
            ApiKeyRateLimitTier.Enterprise,
            12_000);

        usage.CustomerId.Should().Be(customerId);
        usage.Environment.Should().Be(ApiKeyEnvironment.Production);
        usage.RateLimitTier.Should().Be(ApiKeyRateLimitTier.Enterprise);
        usage.RateLimitRequestsPerMinute.Should().Be(12_000);
    }

    private static ApiKeyEntity CreatePublicApiKey(
        Guid customerId,
        ApiKeyEnvironment environment,
        ApiKeyRateLimitTier tier)
    {
        return ApiKeyEntity.Create(
            "partner-public-key",
            ApiKeyHash.FromRawKey("dkh_ptr_old_key_aaaaaaaaaaaaaaaa").Value,
            "dkh_ptr_old_",
            ApiKeyScope.Partner,
            ["orders.read"],
            customerId,
            environment,
            tier,
            "Partner production API key",
            DateTimeOffset.UtcNow.AddDays(30));
    }
}
