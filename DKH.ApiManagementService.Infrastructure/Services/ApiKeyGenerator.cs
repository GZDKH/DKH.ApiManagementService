using System.Security.Cryptography;
using System.Text;
using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Domain.Enums;

namespace DKH.ApiManagementService.Infrastructure.Services;

public class ApiKeyGenerator : IApiKeyGenerator
{
    private const int KeyLength = 32;

    private static readonly Dictionary<ApiKeyScope, string> ScopePrefixes = new()
    {
        [ApiKeyScope.Mcp] = "mcp",
        [ApiKeyScope.Webhook] = "wh",
        [ApiKeyScope.Partner] = "ptr",
        [ApiKeyScope.Storefront] = "sf",
        [ApiKeyScope.Internal] = "int",
    };

    public (string RawKey, string KeyHash, string KeyPrefix) Generate(ApiKeyScope scope)
    {
        var scopePrefix = ScopePrefixes.GetValueOrDefault(scope, "unk");
        var randomPart = GenerateRandomString(KeyLength);
        var rawKey = $"dkh_{scopePrefix}_{randomPart}";
        var keyHash = ComputeHash(rawKey);
        var keyPrefix = $"dkh_{scopePrefix}_{randomPart[..8]}";

        return (rawKey, keyHash, keyPrefix);
    }

    private static string GenerateRandomString(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        var result = new char[length];

        for (var i = 0; i < length; i++)
        {
            result[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
        }

        return new string(result);
    }

    private static string ComputeHash(string input)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(hash);
    }
}
