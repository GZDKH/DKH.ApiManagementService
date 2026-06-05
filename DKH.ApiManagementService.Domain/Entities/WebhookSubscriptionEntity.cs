using DKH.ApiManagementService.Domain.Enums;
using DKH.ApiManagementService.Domain.Services;
using DKH.Platform.Domain.Entities.Auditing;
using DKH.Platform.Domain.Events;

namespace DKH.ApiManagementService.Domain.Entities;

public sealed class WebhookSubscriptionEntity : FullAuditedEntityWithKey<Guid>, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];

    private WebhookSubscriptionEntity()
    {
        Name = string.Empty;
        CallbackUrl = string.Empty;
        Events = [];
        SigningSecretHash = string.Empty;
        SigningSecretPrefix = string.Empty;
    }

    private WebhookSubscriptionEntity(
        Guid? apiKeyId,
        Guid? customerId,
        string name,
        string callbackUrl,
        IReadOnlyList<string> events,
        string signingSecretHash,
        string signingSecretPrefix,
        int retryMaxAttempts,
        int retryBackoffSeconds,
        bool dlqEnabled)
    {
        Id = Guid.NewGuid();
        ApiKeyId = apiKeyId;
        CustomerId = customerId;
        Name = name;
        CallbackUrl = callbackUrl;
        Events = [.. events];
        SigningSecretHash = signingSecretHash;
        SigningSecretPrefix = signingSecretPrefix;
        Status = WebhookSubscriptionStatus.Active;
        RetryMaxAttempts = retryMaxAttempts;
        RetryBackoffSeconds = retryBackoffSeconds;
        DlqEnabled = dlqEnabled;
    }

    public Guid? ApiKeyId { get; private set; }

    public Guid? CustomerId { get; private set; }

    public string Name { get; private set; }

    public string CallbackUrl { get; private set; }

    public List<string> Events { get; private set; }

    public string SigningSecretHash { get; private set; }

    public string SigningSecretPrefix { get; private set; }

    public WebhookSubscriptionStatus Status { get; private set; }

    public int RetryMaxAttempts { get; private set; }

    public int RetryBackoffSeconds { get; private set; }

    public bool DlqEnabled { get; private set; }

    public DateTimeOffset? LastDeliveryAt { get; private set; }

    public bool? LastDeliverySucceeded { get; private set; }

    public int? LastDeliveryStatusCode { get; private set; }

    public string? LastDeliveryError { get; private set; }

    public int FailureCount { get; private set; }

    public DateTimeOffset? LastRotatedAt { get; private set; }

    public int RotationCount { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();

    public static WebhookSubscriptionEntity Create(
        Guid apiKeyId,
        string name,
        string callbackUrl,
        IReadOnlyList<string> events,
        string rawSigningSecret,
        int retryMaxAttempts,
        int retryBackoffSeconds,
        bool dlqEnabled)
    {
        return Create(
            apiKeyId,
            customerId: null,
            name,
            callbackUrl,
            events,
            rawSigningSecret,
            retryMaxAttempts,
            retryBackoffSeconds,
            dlqEnabled);
    }

    public static WebhookSubscriptionEntity Create(
        Guid? apiKeyId,
        Guid? customerId,
        string name,
        string callbackUrl,
        IReadOnlyList<string> events,
        string rawSigningSecret,
        int retryMaxAttempts,
        int retryBackoffSeconds,
        bool dlqEnabled)
    {
        return new WebhookSubscriptionEntity(
            apiKeyId,
            customerId,
            Require(name, nameof(name)),
            NormalizeCallbackUrl(callbackUrl),
            NormalizeEvents(events),
            WebhookSigningSecretHasher.Hash(rawSigningSecret),
            WebhookSigningSecretHasher.GetPrefix(rawSigningSecret),
            ValidateRetryMaxAttempts(retryMaxAttempts),
            ValidateRetryBackoffSeconds(retryBackoffSeconds),
            dlqEnabled);
    }

    public void Update(
        string name,
        string callbackUrl,
        IReadOnlyList<string> events,
        int retryMaxAttempts,
        int retryBackoffSeconds,
        bool dlqEnabled)
    {
        Name = Require(name, nameof(name));
        CallbackUrl = NormalizeCallbackUrl(callbackUrl);
        Events = [.. NormalizeEvents(events)];
        RetryMaxAttempts = ValidateRetryMaxAttempts(retryMaxAttempts);
        RetryBackoffSeconds = ValidateRetryBackoffSeconds(retryBackoffSeconds);
        DlqEnabled = dlqEnabled;
    }

    public void Disable()
    {
        Status = WebhookSubscriptionStatus.Disabled;
    }

    public void RotateSecret(string rawSigningSecret)
    {
        SigningSecretHash = WebhookSigningSecretHasher.Hash(rawSigningSecret);
        SigningSecretPrefix = WebhookSigningSecretHasher.GetPrefix(rawSigningSecret);
        LastRotatedAt = DateTimeOffset.UtcNow;
        RotationCount++;
    }

    public void RecordDelivery(
        bool succeeded,
        int statusCode,
        string? error,
        DateTimeOffset deliveredAt)
    {
        LastDeliveryAt = deliveredAt;
        LastDeliverySucceeded = succeeded;
        LastDeliveryStatusCode = statusCode;
        LastDeliveryError = string.IsNullOrWhiteSpace(error) ? null : error;
        FailureCount = succeeded ? 0 : FailureCount + 1;
    }

    private static string NormalizeCallbackUrl(string callbackUrl)
    {
        var value = Require(callbackUrl, nameof(callbackUrl));

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new ArgumentException("Callback URL must be an absolute HTTPS URL.", nameof(callbackUrl));
        }

        return value;
    }

    private static List<string> NormalizeEvents(IReadOnlyList<string> events)
    {
        var normalized = events
            .Select(e => e.Trim().ToLowerInvariant())
            .Where(e => e.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (normalized.Count == 0)
        {
            throw new ArgumentException("At least one webhook event must be provided.", nameof(events));
        }

        return normalized;
    }

    private static int ValidateRetryMaxAttempts(int value)
    {
        if (value is < 1 or > 20)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Retry max attempts must be between 1 and 20.");
        }

        return value;
    }

    private static int ValidateRetryBackoffSeconds(int value)
    {
        if (value is < 1 or > 86_400)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Retry backoff must be between 1 second and 1 day.");
        }

        return value;
    }

    private static string Require(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{name} must be provided", name);
        }

        return value.Trim();
    }
}
