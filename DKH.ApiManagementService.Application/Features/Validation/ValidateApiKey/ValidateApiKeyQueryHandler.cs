using System.Security.Cryptography;
using System.Text;
using DKH.ApiManagementService.Application.Abstractions;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.Validation.ValidateApiKey;

public sealed class ValidateApiKeyQueryHandler(IApiKeyRepository repository) : IRequestHandler<ValidateApiKeyQuery, ValidateApiKeyResult>
{
    public async Task<ValidateApiKeyResult> Handle(ValidateApiKeyQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RawKey))
        {
            return new ValidateApiKeyResult(false, ErrorReason: "Key is empty");
        }

        var keyHash = ComputeHash(request.RawKey);
        var entity = await repository.GetByKeyHashAsync(keyHash, cancellationToken);

        if (entity is null)
        {
            return new ValidateApiKeyResult(false, ErrorReason: "Key not found");
        }

        if (!entity.IsActive())
        {
            var reason = entity.IsExpired() ? "Key is expired" : "Key is revoked";
            return new ValidateApiKeyResult(false, entity.Id, ErrorReason: reason);
        }

        entity.RecordUsage();
        await repository.UpdateAsync(entity, cancellationToken);

        return new ValidateApiKeyResult(
            true,
            entity.Id,
            entity.Scope,
            entity.Permissions);
    }

    private static string ComputeHash(string input)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(hash);
    }
}
