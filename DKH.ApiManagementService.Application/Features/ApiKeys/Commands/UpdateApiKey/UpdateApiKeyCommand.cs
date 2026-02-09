using DKH.ApiManagementService.Contracts.Models.V1;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Commands.UpdateApiKey;

public sealed record UpdateApiKeyCommand(
    Guid Id,
    string? Name,
    string? Description,
    IReadOnlyList<string>? Permissions,
    DateTimeOffset? ExpiresAt) : IRequest<ApiKey>;
