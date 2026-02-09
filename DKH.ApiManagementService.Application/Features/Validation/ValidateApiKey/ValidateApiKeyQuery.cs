using MediatR;

namespace DKH.ApiManagementService.Application.Features.Validation.ValidateApiKey;

public sealed record ValidateApiKeyQuery(string RawKey) : IRequest<ValidateApiKeyResult>;

public sealed record ValidateApiKeyResult(
    bool IsValid,
    Guid? ApiKeyId = null,
    Domain.Enums.ApiKeyScope? Scope = null,
    IReadOnlyList<string>? Permissions = null,
    string? ErrorReason = null);
