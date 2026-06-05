using DKH.ApiManagementService.Domain.Entities;
using DKH.ApiManagementService.Domain.Enums;
using DKH.ApiManagementService.Domain.Services;
using FluentAssertions;

namespace DKH.ApiManagementService.Tests.Webhooks;

public sealed class WebhookSubscriptionDomainTests
{
    [Fact]
    public void Create_NormalizesEventsAndStoresOnlySecretHash()
    {
        var subscription = WebhookSubscriptionEntity.Create(
            Guid.NewGuid(),
            "Partner ERP",
            "https://partner.example.com/dkh/webhooks",
            [" order.created ", "stock.adjusted", "ORDER.CREATED"],
            "whsec_live_test_secret",
            retryMaxAttempts: 5,
            retryBackoffSeconds: 30,
            dlqEnabled: true);

        subscription.Status.Should().Be(WebhookSubscriptionStatus.Active);
        subscription.Events.Should().Equal("order.created", "stock.adjusted");
        subscription.SigningSecretHash.Should().Be(WebhookSigningSecretHasher.Hash("whsec_live_test_secret"));
        subscription.SigningSecretPrefix.Should().Be("whsec_liv");
        subscription.SigningSecretHash.Should().NotContain("test_secret");
        subscription.RetryMaxAttempts.Should().Be(5);
        subscription.RetryBackoffSeconds.Should().Be(30);
        subscription.DlqEnabled.Should().BeTrue();
    }

    [Fact]
    public void UpdateAndDisable_ChangeRoutingAndLifecycleState()
    {
        var subscription = CreateSubscription();

        subscription.Update(
            "Partner WMS",
            "https://wms.example.com/hooks",
            ["inventory.low"],
            retryMaxAttempts: 8,
            retryBackoffSeconds: 120,
            dlqEnabled: false);
        subscription.Disable();

        subscription.Name.Should().Be("Partner WMS");
        subscription.CallbackUrl.Should().Be("https://wms.example.com/hooks");
        subscription.Events.Should().Equal("inventory.low");
        subscription.RetryMaxAttempts.Should().Be(8);
        subscription.RetryBackoffSeconds.Should().Be(120);
        subscription.DlqEnabled.Should().BeFalse();
        subscription.Status.Should().Be(WebhookSubscriptionStatus.Disabled);
    }

    [Fact]
    public void Create_WithHttpCallbackUrl_Throws()
    {
        var act = () => WebhookSubscriptionEntity.Create(
            Guid.NewGuid(),
            "Partner ERP",
            "http://partner.example.com/dkh/webhooks",
            ["order.created"],
            "whsec_live_test_secret",
            retryMaxAttempts: 5,
            retryBackoffSeconds: 30,
            dlqEnabled: true);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*HTTPS*");
    }

    [Fact]
    public void RotateSecret_UpdatesHashPrefixAndRotationTelemetry()
    {
        var subscription = CreateSubscription();

        subscription.RotateSecret("whsec_rotated_secret");

        subscription.SigningSecretHash.Should().Be(WebhookSigningSecretHasher.Hash("whsec_rotated_secret"));
        subscription.SigningSecretPrefix.Should().Be("whsec_rot");
        subscription.RotationCount.Should().Be(1);
        subscription.LastRotatedAt.Should().NotBeNull();
    }

    [Fact]
    public void RecordDelivery_TracksLastAttemptAndFailureCount()
    {
        var subscription = CreateSubscription();
        var deliveredAt = new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);

        subscription.RecordDelivery(false, 503, "Service unavailable", deliveredAt);

        subscription.LastDeliveryAt.Should().Be(deliveredAt);
        subscription.LastDeliverySucceeded.Should().BeFalse();
        subscription.LastDeliveryStatusCode.Should().Be(503);
        subscription.LastDeliveryError.Should().Be("Service unavailable");
        subscription.FailureCount.Should().Be(1);

        subscription.RecordDelivery(true, 200, null, deliveredAt.AddMinutes(1));

        subscription.LastDeliverySucceeded.Should().BeTrue();
        subscription.LastDeliveryStatusCode.Should().Be(200);
        subscription.LastDeliveryError.Should().BeNull();
        subscription.FailureCount.Should().Be(0);
    }

    [Fact]
    public void SignPayload_ProducesDeterministicWebhookSignature()
    {
        var signature = WebhookSignatureService.SignPayload(
            "whsec_live_test_secret",
            "2026-06-05T12:00:00Z",
                                 /*lang=json,strict*/
                                 """{"event":"order.created"}""");

        signature.Should().Be("sha256=61eb8ccbf4adf3a8a024ab3fba215311259ff5e969d3eacca8d033b127ee6715");
    }

    private static WebhookSubscriptionEntity CreateSubscription()
    {
        return WebhookSubscriptionEntity.Create(
            Guid.NewGuid(),
            "Partner ERP",
            "https://partner.example.com/dkh/webhooks",
            ["order.created"],
            "whsec_live_test_secret",
            retryMaxAttempts: 5,
            retryBackoffSeconds: 30,
            dlqEnabled: true);
    }
}
