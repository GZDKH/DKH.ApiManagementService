using System.Security.Cryptography;
using System.Text;
using DKH.ApiManagementService.Application.Abstractions;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.Validation.CheckPermission;

public sealed class CheckPermissionQueryHandler(IApiKeyRepository repository) : IRequestHandler<CheckPermissionQuery, CheckPermissionResult>
{
    public async Task<CheckPermissionResult> Handle(CheckPermissionQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RawKey))
        {
            return new CheckPermissionResult(false, "Key is empty");
        }

        var keyHash = ComputeHash(request.RawKey);
        var entity = await repository.GetByKeyHashAsync(keyHash, cancellationToken);

        if (entity is null)
        {
            return new CheckPermissionResult(false, "Key not found");
        }

        if (!entity.IsActive())
        {
            var reason = entity.IsExpired() ? "Key is expired" : "Key is revoked";
            return new CheckPermissionResult(false, reason);
        }

        if (!entity.HasPermission(request.RequiredPermission))
        {
            return new CheckPermissionResult(false, $"Permission '{request.RequiredPermission}' not granted");
        }

        return new CheckPermissionResult(true);
    }

    private static string ComputeHash(string input)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(hash);
    }
}
