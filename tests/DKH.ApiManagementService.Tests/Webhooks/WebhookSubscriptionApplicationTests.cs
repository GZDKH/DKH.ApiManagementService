using DKH.ApiManagementService.Application.Features.Webhooks.Commands.CreateWebhookSubscription;
using DKH.ApiManagementService.Application.Features.Webhooks.Commands.DisableWebhookSubscription;
using DKH.ApiManagementService.Application.Features.Webhooks.Commands.RotateWebhookSubscriptionSecret;
using DKH.ApiManagementService.Application.Features.Webhooks.Commands.UpdateWebhookSubscription;
using DKH.ApiManagementService.Application.Features.Webhooks.Queries.GetWebhookSubscription;
using DKH.ApiManagementService.Application.Features.Webhooks.Queries.ListWebhookSubscriptions;
using DKH.ApiManagementService.Domain.Enums;
using DKH.ApiManagementService.Domain.Services;
using DKH.ApiManagementService.Infrastructure.Persistence;
using DKH.Platform.Identity;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace DKH.ApiManagementService.Tests.Webhooks;

public sealed class WebhookSubscriptionApplicationTests
{
    [Fact]
    public async Task CreateAndList_PersistsSubscriptionWithoutRawSecretAsync()
    {
        var apiKeyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        await using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var createHandler = new CreateWebhookSubscriptionCommandHandler(dbContext);
        var listHandler = new ListWebhookSubscriptionsQueryHandler(dbContext);

        var created = await createHandler.Handle(
            new CreateWebhookSubscriptionCommand(
                "Partner ERP",
                "https://partner.example.com/webhooks",
                ["order.created", "stock.adjusted"],
                "whsec_live_test_secret",
                RetryMaxAttempts: 5,
                RetryBackoffSeconds: 30,
                DlqEnabled: true,
                ApiKeyId: apiKeyId,
                CustomerId: customerId),
            CancellationToken.None);

        var listed = await listHandler.Handle(
            new ListWebhookSubscriptionsQuery(apiKeyId, customerId, Page: 1, PageSize: 20),
            CancellationToken.None);

        created.Subscription.Id.Should().NotBeEmpty();
        created.Subscription.SigningSecretPrefix.Should().Be("whsec_liv");
        typeof(Application.Features.Webhooks.WebhookSubscriptionDto)
            .GetProperty("SigningSecretHash")
            .Should()
            .BeNull("webhook responses must not expose verifier material");
        listed.TotalCount.Should().Be(1);
        listed.Subscriptions.Should().ContainSingle(x => x.Id == created.Subscription.Id);
        (await dbContext.WebhookSubscriptions.SingleAsync()).SigningSecretHash.Should()
            .Be(WebhookSigningSecretHasher.Hash("whsec_live_test_secret"));
    }

    [Fact]
    public async Task UpdateDisableAndRotate_ChangesLifecycleStateAsync()
    {
        var apiKeyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        await using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var createHandler = new CreateWebhookSubscriptionCommandHandler(dbContext);
        var updateHandler = new UpdateWebhookSubscriptionCommandHandler(dbContext);
        var disableHandler = new DisableWebhookSubscriptionCommandHandler(dbContext);
        var rotateHandler = new RotateWebhookSubscriptionSecretCommandHandler(dbContext);
        var getHandler = new GetWebhookSubscriptionQueryHandler(dbContext);

        var created = await createHandler.Handle(
            new CreateWebhookSubscriptionCommand(
                "Partner ERP",
                "https://partner.example.com/webhooks",
                ["order.created"],
                "whsec_live_test_secret",
                RetryMaxAttempts: 5,
                RetryBackoffSeconds: 30,
                DlqEnabled: true,
                ApiKeyId: apiKeyId,
                CustomerId: customerId),
            CancellationToken.None);

        await updateHandler.Handle(
            new UpdateWebhookSubscriptionCommand(
                created.Subscription.Id,
                "Partner WMS",
                "https://wms.example.com/hooks",
                ["inventory.low"],
                RetryMaxAttempts: 8,
                RetryBackoffSeconds: 120,
                DlqEnabled: false,
                ApiKeyId: apiKeyId,
                CustomerId: customerId),
            CancellationToken.None);
        await rotateHandler.Handle(
            new RotateWebhookSubscriptionSecretCommand(created.Subscription.Id, "whsec_rotated_secret", apiKeyId, customerId),
            CancellationToken.None);
        await disableHandler.Handle(
            new DisableWebhookSubscriptionCommand(created.Subscription.Id, apiKeyId, customerId),
            CancellationToken.None);

        var result = await getHandler.Handle(
            new GetWebhookSubscriptionQuery(created.Subscription.Id, apiKeyId, customerId),
            CancellationToken.None);

        result.Subscription.Should().NotBeNull();
        var subscription = result.Subscription!;
        subscription.Name.Should().Be("Partner WMS");
        subscription.CallbackUrl.Should().Be("https://wms.example.com/hooks");
        subscription.Events.Should().Equal("inventory.low");
        subscription.RetryMaxAttempts.Should().Be(8);
        subscription.RetryBackoffSeconds.Should().Be(120);
        subscription.DlqEnabled.Should().BeFalse();
        subscription.SigningSecretPrefix.Should().Be("whsec_rot");
        subscription.RotationCount.Should().Be(1);
        subscription.LastRotatedAt.Should().NotBeNull();
        subscription.Status.Should().Be(WebhookSubscriptionStatus.Disabled);
    }

    [Fact]
    public async Task Get_WhenSubscriptionIsOwnedByAnotherApiKey_ReturnsNullAsync()
    {
        await using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var createHandler = new CreateWebhookSubscriptionCommandHandler(dbContext);
        var getHandler = new GetWebhookSubscriptionQueryHandler(dbContext);

        var created = await createHandler.Handle(
            new CreateWebhookSubscriptionCommand(
                "Partner ERP",
                "https://partner.example.com/webhooks",
                ["order.created"],
                "whsec_live_test_secret",
                RetryMaxAttempts: 5,
                RetryBackoffSeconds: 30,
                DlqEnabled: true,
                ApiKeyId: Guid.NewGuid(),
                CustomerId: Guid.NewGuid()),
            CancellationToken.None);

        var result = await getHandler.Handle(
            new GetWebhookSubscriptionQuery(created.Subscription.Id, Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        result.Subscription.Should().BeNull();
    }

    [Fact]
    public async Task Create_WithoutApiKeyId_ThrowsAsync()
    {
        await using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var createHandler = new CreateWebhookSubscriptionCommandHandler(dbContext);

        var create = () => createHandler.Handle(
            new CreateWebhookSubscriptionCommand(
                "Partner ERP",
                "https://partner.example.com/webhooks",
                ["order.created"],
                "whsec_live_test_secret",
                RetryMaxAttempts: 5,
                RetryBackoffSeconds: 30,
                DlqEnabled: true,
                ApiKeyId: null,
                CustomerId: Guid.NewGuid()),
            CancellationToken.None);

        await create.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task OwnerScopedOperations_WithoutOwnerContext_DoNotExposeSubscriptionsAsync()
    {
        await using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var createHandler = new CreateWebhookSubscriptionCommandHandler(dbContext);
        var listHandler = new ListWebhookSubscriptionsQueryHandler(dbContext);
        var getHandler = new GetWebhookSubscriptionQueryHandler(dbContext);
        var updateHandler = new UpdateWebhookSubscriptionCommandHandler(dbContext);
        var disableHandler = new DisableWebhookSubscriptionCommandHandler(dbContext);
        var rotateHandler = new RotateWebhookSubscriptionSecretCommandHandler(dbContext);

        var created = await createHandler.Handle(
            new CreateWebhookSubscriptionCommand(
                "Partner ERP",
                "https://partner.example.com/webhooks",
                ["order.created"],
                "whsec_live_test_secret",
                RetryMaxAttempts: 5,
                RetryBackoffSeconds: 30,
                DlqEnabled: true,
                ApiKeyId: Guid.NewGuid(),
                CustomerId: Guid.NewGuid()),
            CancellationToken.None);

        var listed = await listHandler.Handle(
            new ListWebhookSubscriptionsQuery(ApiKeyId: null, CustomerId: null, Page: 1, PageSize: 20),
            CancellationToken.None);
        var got = await getHandler.Handle(
            new GetWebhookSubscriptionQuery(created.Subscription.Id, ApiKeyId: null, CustomerId: null),
            CancellationToken.None);
        Func<Task> update = async () => await updateHandler.Handle(
            new UpdateWebhookSubscriptionCommand(
                created.Subscription.Id,
                "Partner WMS",
                "https://wms.example.com/hooks",
                ["inventory.low"],
                RetryMaxAttempts: 8,
                RetryBackoffSeconds: 120,
                DlqEnabled: false,
                ApiKeyId: null,
                CustomerId: null),
            CancellationToken.None);
        Func<Task> disable = () => disableHandler.Handle(
            new DisableWebhookSubscriptionCommand(created.Subscription.Id, ApiKeyId: null, CustomerId: null),
            CancellationToken.None);
        Func<Task> rotate = async () => await rotateHandler.Handle(
            new RotateWebhookSubscriptionSecretCommand(
                created.Subscription.Id,
                "whsec_rotated_secret",
                ApiKeyId: null,
                CustomerId: null),
            CancellationToken.None);

        listed.TotalCount.Should().Be(0);
        listed.Subscriptions.Should().BeEmpty();
        got.Subscription.Should().BeNull();
        await update.Should().ThrowAsync<KeyNotFoundException>();
        await disable.Should().ThrowAsync<KeyNotFoundException>();
        await rotate.Should().ThrowAsync<KeyNotFoundException>();
    }

    private static AsyncServiceScope CreateScope()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Substitute.For<IPlatformCurrentUser>());
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase($"webhook-subscriptions-{Guid.NewGuid()}"));

        var provider = services.BuildServiceProvider();
        return provider.CreateAsyncScope();
    }
}
