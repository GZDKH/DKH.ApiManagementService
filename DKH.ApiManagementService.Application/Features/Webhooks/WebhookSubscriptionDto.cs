using DKH.ApiManagementService.Domain.Entities;
using DKH.ApiManagementService.Domain.Enums;

namespace DKH.ApiManagementService.Application.Features.Webhooks;

public sealed record WebhookSubscriptionDto(
    Guid Id,
    Guid? ApiKeyId,
    Guid? CustomerId,
    string Name,
    string CallbackUrl,
    IReadOnlyList<string> Events,
    string SigningSecretPrefix,
    WebhookSubscriptionStatus Status,
    int RetryMaxAttempts,
    int RetryBackoffSeconds,
    bool DlqEnabled,
    DateTimeOffset? LastDeliveryAt,
    bool? LastDeliverySucceeded,
    int? LastDeliveryStatusCode,
    string? LastDeliveryError,
    int FailureCount,
    DateTimeOffset? LastRotatedAt,
    int RotationCount);

public static class WebhookSubscriptionMapper
{
    public static WebhookSubscriptionDto ToDto(this WebhookSubscriptionEntity entity)
    {
        return new WebhookSubscriptionDto(
            entity.Id,
            entity.ApiKeyId,
            entity.CustomerId,
            entity.Name,
            entity.CallbackUrl,
            [.. entity.Events],
            entity.SigningSecretPrefix,
            entity.Status,
            entity.RetryMaxAttempts,
            entity.RetryBackoffSeconds,
            entity.DlqEnabled,
            entity.LastDeliveryAt,
            entity.LastDeliverySucceeded,
            entity.LastDeliveryStatusCode,
            entity.LastDeliveryError,
            entity.FailureCount,
            entity.LastRotatedAt,
            entity.RotationCount);
    }
}
