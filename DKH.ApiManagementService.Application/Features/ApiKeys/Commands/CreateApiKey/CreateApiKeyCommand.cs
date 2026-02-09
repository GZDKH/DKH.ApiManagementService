using DKH.ApiManagementService.Contracts.Models.V1;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Commands.CreateApiKey;

public sealed record CreateApiKeyCommand(
    string Name,
    Domain.Enums.ApiKeyScope Scope,
    IReadOnlyList<string> Permissions,
    string CreatedBy,
    string? Description = null,
    DateTimeOffset? ExpiresAt = null) : IRequest<CreateApiKeyResult>;

public sealed record CreateApiKeyResult(ApiKey ApiKey, string RawKey);
