using DKH.ApiManagementService.Domain.Enums;

namespace DKH.ApiManagementService.Domain.Services;

public static class ApiKeyRateLimitPolicy
{
    public static int GetRequestsPerMinute(ApiKeyEnvironment environment, ApiKeyRateLimitTier tier)
    {
        var tierLimit = tier switch
        {
            ApiKeyRateLimitTier.Development => 60,
            ApiKeyRateLimitTier.Standard => 600,
            ApiKeyRateLimitTier.Professional => 3_000,
            ApiKeyRateLimitTier.Enterprise => 12_000,
            _ => 600,
        };

        return environment == ApiKeyEnvironment.Sandbox
            ? Math.Min(tierLimit, 120)
            : tierLimit;
    }
}
