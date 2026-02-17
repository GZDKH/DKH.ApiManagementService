using DKH.ApiManagementService.Contracts.ApiManagement.Models.ApiKey.v1;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Commands.UpdateApiKey;

public sealed record UpdateApiKeyCommand(
    Guid Id,
    string? Name,
    string? Description,
    IReadOnlyList<string>? Permissions,
    DateTimeOffset? ExpiresAt) : IRequest<ApiKeyModel>;
