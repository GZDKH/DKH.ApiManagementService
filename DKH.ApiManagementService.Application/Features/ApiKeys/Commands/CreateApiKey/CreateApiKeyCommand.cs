using DKH.ApiManagementService.Contracts.ApiManagement.Models.ApiKey.v1;
using MediatR;
using DomainEnvironment = DKH.ApiManagementService.Domain.Enums.ApiKeyEnvironment;
using DomainRateLimitTier = DKH.ApiManagementService.Domain.Enums.ApiKeyRateLimitTier;
using DomainScope = DKH.ApiManagementService.Domain.Enums.ApiKeyScope;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Commands.CreateApiKey;

public sealed record CreateApiKeyCommand(
    string Name,
    DomainScope Scope,
    IReadOnlyList<string> Permissions,
    string? Description = null,
    DateTimeOffset? ExpiresAt = null,
    Guid? CustomerId = null,
    Guid? StorefrontId = null,
    DomainEnvironment Environment = DomainEnvironment.Production,
    DomainRateLimitTier RateLimitTier = DomainRateLimitTier.Standard) : IRequest<CreateApiKeyResult>;

public sealed record CreateApiKeyResult(ApiKeyModel ApiKey, string RawKey);
