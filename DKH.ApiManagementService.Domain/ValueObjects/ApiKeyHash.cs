using System.Security.Cryptography;
using System.Text;
using DKH.Platform.Domain.Values;

namespace DKH.ApiManagementService.Domain.ValueObjects;

public sealed class ApiKeyHash : ValueObject
{
    public string Value { get; }

    private ApiKeyHash(string value)
    {
        Value = value;
    }

    public static ApiKeyHash FromRawKey(string rawKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawKey);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(rawKey));
        return new ApiKeyHash(Convert.ToHexStringLower(hash));
    }

    public static ApiKeyHash FromHash(string hash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hash);
        return new ApiKeyHash(hash);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }
}
