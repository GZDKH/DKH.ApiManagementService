using System.Security.Claims;
using DKH.ApiManagementService.Api.Auth;
using DKH.ApiManagementService.Api.Controllers.Webhooks.V1;
using DKH.ApiManagementService.Application.Features.Webhooks;
using DKH.ApiManagementService.Application.Features.Webhooks.Commands.CreateWebhookSubscription;
using DKH.ApiManagementService.Domain.Enums;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace DKH.ApiManagementService.Tests.Webhooks;

public sealed class WebhookSubscriptionsControllerTests
{
    [Fact]
    public async Task CreateAsync_WithWebhookPermission_SendsOwnerContextAsync()
    {
        var apiKeyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        CreateWebhookSubscriptionCommand? observedCommand = null;
        var subscription = CreateDto(apiKeyId, customerId);
        mediator
            .Send(Arg.Do<CreateWebhookSubscriptionCommand>(command => observedCommand = command), Arg.Any<CancellationToken>())
            .Returns(new CreateWebhookSubscriptionResult(subscription));

        var controller = CreateController(mediator, apiKeyId, customerId, "webhooks:subscribe");

        var response = await controller.CreateAsync(
            new CreateWebhookSubscriptionRequest(
                "Partner ERP",
                "https://partner.example.com/webhooks",
                ["order.created"],
                "whsec_live_test_secret",
                RetryMaxAttempts: 5,
                RetryBackoffSeconds: 30,
                DlqEnabled: true),
            CancellationToken.None);

        var created = response.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(WebhookSubscriptionsController.GetAsync));
        created.Value.Should().Be(subscription);
        observedCommand.Should().NotBeNull();
        observedCommand!.ApiKeyId.Should().Be(apiKeyId);
        observedCommand.CustomerId.Should().Be(customerId);
    }

    [Fact]
    public async Task CreateAsync_WithoutWebhookPermission_ReturnsForbidAsync()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator, Guid.NewGuid(), Guid.NewGuid(), "orders.read");

        var response = await controller.CreateAsync(
            new CreateWebhookSubscriptionRequest(
                "Partner ERP",
                "https://partner.example.com/webhooks",
                ["order.created"],
                "whsec_live_test_secret",
                RetryMaxAttempts: 5,
                RetryBackoffSeconds: 30,
                DlqEnabled: true),
            CancellationToken.None);

        response.Result.Should().BeOfType<ForbidResult>();
        await mediator.DidNotReceive().Send(Arg.Any<CreateWebhookSubscriptionCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithWebhookPermissionButMissingApiKeyId_ReturnsForbidAsync()
    {
        var mediator = Substitute.For<IMediator>();
        var subscription = CreateDto(Guid.NewGuid(), Guid.NewGuid());
        mediator
            .Send(Arg.Any<CreateWebhookSubscriptionCommand>(), Arg.Any<CancellationToken>())
            .Returns(new CreateWebhookSubscriptionResult(subscription));
        var controller = CreateController(mediator, apiKeyId: null, Guid.NewGuid(), "webhooks:subscribe");

        var response = await controller.CreateAsync(
            new CreateWebhookSubscriptionRequest(
                "Partner ERP",
                "https://partner.example.com/webhooks",
                ["order.created"],
                "whsec_live_test_secret",
                RetryMaxAttempts: 5,
                RetryBackoffSeconds: 30,
                DlqEnabled: true),
            CancellationToken.None);

        response.Result.Should().BeOfType<ForbidResult>();
        await mediator.DidNotReceive().Send(Arg.Any<CreateWebhookSubscriptionCommand>(), Arg.Any<CancellationToken>());
    }

    private static WebhookSubscriptionsController CreateController(
        IMediator mediator,
        Guid? apiKeyId,
        Guid? customerId,
        string permission)
    {
        var claims = new List<Claim>
        {
            new(ApiKeyValidator.PermissionClaimType, permission),
        };

        if (apiKeyId.HasValue)
        {
            claims.Add(new Claim(ApiKeyValidator.ApiKeyIdClaimType, apiKeyId.Value.ToString()));
        }

        if (customerId.HasValue)
        {
            claims.Add(new Claim(ApiKeyValidator.CustomerIdClaimType, customerId.Value.ToString()));
        }

        var user = new ClaimsPrincipal(new ClaimsIdentity(
            claims,
            authenticationType: "ApiKey"));

        return new WebhookSubscriptionsController(mediator)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user,
                },
            },
        };
    }

    private static WebhookSubscriptionDto CreateDto(Guid apiKeyId, Guid customerId)
    {
        return new WebhookSubscriptionDto(
            Guid.NewGuid(),
            apiKeyId,
            customerId,
            "Partner ERP",
            "https://partner.example.com/webhooks",
            ["order.created"],
            "whsec_liv",
            WebhookSubscriptionStatus.Active,
            5,
            30,
            true,
            LastDeliveryAt: null,
            LastDeliverySucceeded: null,
            LastDeliveryStatusCode: null,
            LastDeliveryError: null,
            FailureCount: 0,
            LastRotatedAt: null,
            RotationCount: 0);
    }
}
