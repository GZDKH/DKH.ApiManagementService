using System.Security.Cryptography;
using System.Text;

namespace DKH.ApiManagementService.Domain.Services;

public static class WebhookSigningSecretHasher
{
    public static string Hash(string rawSecret)
    {
        if (string.IsNullOrWhiteSpace(rawSecret))
        {
            throw new ArgumentException("Signing secret must be provided", nameof(rawSecret));
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawSecret));
        return Convert.ToHexStringLower(bytes);
    }

    public static string GetPrefix(string rawSecret)
    {
        if (string.IsNullOrWhiteSpace(rawSecret))
        {
            throw new ArgumentException("Signing secret must be provided", nameof(rawSecret));
        }

        return rawSecret.Length <= 9 ? rawSecret : rawSecret[..9];
    }
}
