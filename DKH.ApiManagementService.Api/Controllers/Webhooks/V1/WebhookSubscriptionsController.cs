using System.Security.Claims;
using DKH.ApiManagementService.Api.Auth;
using DKH.ApiManagementService.Application.Features.Webhooks;
using DKH.ApiManagementService.Application.Features.Webhooks.Commands.CreateWebhookSubscription;
using DKH.ApiManagementService.Application.Features.Webhooks.Commands.DisableWebhookSubscription;
using DKH.ApiManagementService.Application.Features.Webhooks.Commands.RotateWebhookSubscriptionSecret;
using DKH.ApiManagementService.Application.Features.Webhooks.Commands.UpdateWebhookSubscription;
using DKH.ApiManagementService.Application.Features.Webhooks.Queries.GetWebhookSubscription;
using DKH.ApiManagementService.Application.Features.Webhooks.Queries.ListWebhookSubscriptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DKH.ApiManagementService.Api.Controllers.Webhooks.V1;

[ApiController]
[Authorize]
[ApiExplorerSettings(GroupName = "api-management")]
[Route("api/v1/webhook-subscriptions")]
public sealed class WebhookSubscriptionsController(IMediator mediator) : ControllerBase
{
    private const string SubscribePermission = "webhooks:subscribe";
    private const string ManagePermission = "webhooks.manage";

    [HttpGet]
    [ProducesResponseType(typeof(ListWebhookSubscriptionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ListWebhookSubscriptionsResponse>> ListAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!CanManageWebhooks())
        {
            return Forbid();
        }

        var result = await mediator.Send(
            new ListWebhookSubscriptionsQuery(GetApiKeyId(), GetCustomerId(), page, pageSize),
            cancellationToken);

        return new ListWebhookSubscriptionsResponse(result.Subscriptions, result.TotalCount);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WebhookSubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WebhookSubscriptionDto>> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!CanManageWebhooks())
        {
            return Forbid();
        }

        var result = await mediator.Send(
            new GetWebhookSubscriptionQuery(id, GetApiKeyId(), GetCustomerId()),
            cancellationToken);

        return result.Subscription is null ? NotFound() : result.Subscription;
    }

    [HttpPost]
    [ProducesResponseType(typeof(WebhookSubscriptionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<WebhookSubscriptionDto>> CreateAsync(
        CreateWebhookSubscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!CanManageWebhooks())
        {
            return Forbid();
        }

        var result = await mediator.Send(
            new CreateWebhookSubscriptionCommand(
                request.Name,
                request.CallbackUrl,
                request.Events,
                request.SigningSecret,
                request.RetryMaxAttempts,
                request.RetryBackoffSeconds,
                request.DlqEnabled,
                GetApiKeyId(),
                GetCustomerId()),
            cancellationToken);

        return CreatedAtAction(nameof(GetAsync), new { id = result.Subscription.Id }, result.Subscription);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(WebhookSubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WebhookSubscriptionDto>> UpdateAsync(
        Guid id,
        UpdateWebhookSubscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!CanManageWebhooks())
        {
            return Forbid();
        }

        try
        {
            var result = await mediator.Send(
                new UpdateWebhookSubscriptionCommand(
                    id,
                    request.Name,
                    request.CallbackUrl,
                    request.Events,
                    request.RetryMaxAttempts,
                    request.RetryBackoffSeconds,
                    request.DlqEnabled,
                    GetApiKeyId(),
                    GetCustomerId()),
                cancellationToken);

            return result.Subscription;
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:guid}/disable")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DisableAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!CanManageWebhooks())
        {
            return Forbid();
        }

        try
        {
            await mediator.Send(new DisableWebhookSubscriptionCommand(id, GetApiKeyId(), GetCustomerId()), cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:guid}/rotate-secret")]
    [ProducesResponseType(typeof(WebhookSubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WebhookSubscriptionDto>> RotateSecretAsync(
        Guid id,
        RotateWebhookSubscriptionSecretRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!CanManageWebhooks())
        {
            return Forbid();
        }

        try
        {
            var result = await mediator.Send(
                new RotateWebhookSubscriptionSecretCommand(id, request.SigningSecret, GetApiKeyId(), GetCustomerId()),
                cancellationToken);

            return result.Subscription;
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    private bool CanManageWebhooks()
    {
        return GetApiKeyId().HasValue &&
               (User.HasClaim(ApiKeyValidator.PermissionClaimType, SubscribePermission) ||
                User.HasClaim(ApiKeyValidator.PermissionClaimType, ManagePermission));
    }

    private Guid? GetApiKeyId()
    {
        return TryGetGuidClaim(ApiKeyValidator.ApiKeyIdClaimType);
    }

    private Guid? GetCustomerId()
    {
        return TryGetGuidClaim(ApiKeyValidator.CustomerIdClaimType);
    }

    private Guid? TryGetGuidClaim(string claimType)
    {
        var value = User.FindFirstValue(claimType);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}

public sealed record ListWebhookSubscriptionsResponse(
    IReadOnlyList<WebhookSubscriptionDto> Subscriptions,
    int TotalCount);

public sealed record CreateWebhookSubscriptionRequest(
    string Name,
    string CallbackUrl,
    IReadOnlyList<string> Events,
    string SigningSecret,
    int RetryMaxAttempts = 5,
    int RetryBackoffSeconds = 30,
    bool DlqEnabled = true);

public sealed record UpdateWebhookSubscriptionRequest(
    string Name,
    string CallbackUrl,
    IReadOnlyList<string> Events,
    int RetryMaxAttempts = 5,
    int RetryBackoffSeconds = 30,
    bool DlqEnabled = true);

public sealed record RotateWebhookSubscriptionSecretRequest(string SigningSecret);
