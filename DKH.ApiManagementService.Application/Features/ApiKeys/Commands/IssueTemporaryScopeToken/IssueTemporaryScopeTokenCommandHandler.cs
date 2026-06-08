using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Domain.Entities;
using DKH.ApiManagementService.Domain.Enums;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Commands.IssueTemporaryScopeToken;

public sealed class IssueTemporaryScopeTokenCommandHandler(
    IApiKeyRepository repository,
    IApiKeyGenerator keyGenerator) : IRequestHandler<IssueTemporaryScopeTokenCommand, IssueTemporaryScopeTokenResult>
{
    public async Task<IssueTemporaryScopeTokenResult> Handle(
        IssueTemporaryScopeTokenCommand request,
        CancellationToken cancellationToken)
    {
        var expiresAt = DateTimeOffset.UtcNow.Add(request.Ttl);
        var (rawKey, keyHash, keyPrefix) = keyGenerator.Generate(ApiKeyScope.Internal);

        var entity = ApiKeyEntity.Create(
            $"temp-scope:{request.SubjectId}",
            keyHash,
            keyPrefix,
            ApiKeyScope.Internal,
            request.Permissions,
            customerId: null,
            ApiKeyEnvironment.Production,
            ApiKeyRateLimitTier.Development,
            BuildDescription(request),
            expiresAt);

        await repository.AddAsync(entity, cancellationToken);

        return new IssueTemporaryScopeTokenResult(rawKey, expiresAt);
    }

    private static string BuildDescription(IssueTemporaryScopeTokenCommand request)
    {
        var reason = string.IsNullOrWhiteSpace(request.Reason)
            ? "temporary scoped access"
            : request.Reason.Trim();

        return $"Temporary scoped token for {request.SubjectId}; resource={request.ResourceType}:{request.ResourceId}; reason={reason}";
    }
}
