using DKH.Platform.Domain.Entities;

namespace DKH.ApiManagementService.Domain.Entities;

public class ApiKeyUsageEntity : Entity<Guid>
{
    private ApiKeyUsageEntity()
    {
    }

    public ApiKeyUsageEntity(
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

    public string Endpoint { get; private set; } = null!;

    public int StatusCode { get; private set; }

    public string? IpAddress { get; private set; }

    public string? UserAgent { get; private set; }

    public DateTimeOffset Timestamp { get; private set; }

    public long ResponseTimeMs { get; private set; }

    public ApiKeyEntity ApiKey { get; private set; } = null!;
}
