using DKH.Platform.Domain.Entities;

namespace DKH.ApiManagementService.Domain.Entities;

public sealed class ApiKeyUsageEntity : Entity<Guid>
{
    private ApiKeyUsageEntity()
    {
        Endpoint = string.Empty;
    }

    private ApiKeyUsageEntity(
        Guid apiKeyId,
        string endpoint,
        int statusCode,
        string? ipAddress,
        string? userAgent,
        long responseTimeMs)
    {
        Id = Guid.NewGuid();
        ApiKeyId = apiKeyId;
        Endpoint = endpoint;
        StatusCode = statusCode;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        ResponseTimeMs = responseTimeMs;
        Timestamp = DateTimeOffset.UtcNow;
    }

    public Guid ApiKeyId { get; private set; }

    public string Endpoint { get; private set; }

    public int StatusCode { get; private set; }

    public string? IpAddress { get; private set; }

    public string? UserAgent { get; private set; }

    public DateTimeOffset Timestamp { get; private set; }

    public long ResponseTimeMs { get; private set; }

    public ApiKeyEntity ApiKey { get; private set; } = null!;

    public static ApiKeyUsageEntity Create(
        Guid apiKeyId,
        string endpoint,
        int statusCode,
        string? ipAddress,
        string? userAgent,
        long responseTimeMs)
    {
        if (apiKeyId == Guid.Empty)
        {
            throw new ArgumentException("API key ID must be provided", nameof(apiKeyId));
        }

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new ArgumentException("Endpoint must be provided", nameof(endpoint));
        }

        return new ApiKeyUsageEntity(apiKeyId, endpoint, statusCode, ipAddress, userAgent, responseTimeMs);
    }
}
