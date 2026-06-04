using DKH.ApiManagementService.Contracts.ApiManagement.Models.ApiKey.v1;
using MediatR;
using DomainEnvironment = DKH.ApiManagementService.Domain.Enums.ApiKeyEnvironment;
using DomainRateLimitTier = DKH.ApiManagementService.Domain.Enums.ApiKeyRateLimitTier;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Commands.UpdateApiKey;

public sealed record UpdateApiKeyCommand(
    Guid Id,
    string? Name,
    string? Description,
    IReadOnlyList<string>? Permissions,
    DateTimeOffset? ExpiresAt,
    Guid? CustomerId = null,
    DomainEnvironment? Environment = null,
    DomainRateLimitTier? RateLimitTier = null) : IRequest<ApiKeyModel>;
