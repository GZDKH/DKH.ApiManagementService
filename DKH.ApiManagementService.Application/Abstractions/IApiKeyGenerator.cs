using DKH.ApiManagementService.Domain.Enums;

namespace DKH.ApiManagementService.Application.Abstractions;

public interface IApiKeyGenerator
{
    (string RawKey, string KeyHash, string KeyPrefix) Generate(ApiKeyScope scope);
}
