using DKH.ApiManagementService.Contracts.ApiManagement.Models.ApiKey.v1;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Commands.CreateApiKey;

public sealed record CreateApiKeyCommand(
    string Name,
    Domain.Enums.ApiKeyScope Scope,
    IReadOnlyList<string> Permissions,
    string? Description = null,
    DateTimeOffset? ExpiresAt = null) : IRequest<CreateApiKeyResult>;

public sealed record CreateApiKeyResult(ApiKeyModel ApiKey, string RawKey);
