using System.Security.Cryptography;
using System.Text;

namespace DKH.ApiManagementService.Domain.Services;

public static class WebhookSignatureService
{
    public static string SignPayload(string rawSecret, string timestamp, string payload)
    {
        if (string.IsNullOrWhiteSpace(rawSecret))
        {
            throw new ArgumentException("Signing secret must be provided", nameof(rawSecret));
        }

        if (string.IsNullOrWhiteSpace(timestamp))
        {
            throw new ArgumentException("Timestamp must be provided", nameof(timestamp));
        }

        var signedPayload = $"{timestamp}.{payload ?? string.Empty}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(rawSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload));
        return $"sha256={Convert.ToHexStringLower(hash)}";
    }
}
