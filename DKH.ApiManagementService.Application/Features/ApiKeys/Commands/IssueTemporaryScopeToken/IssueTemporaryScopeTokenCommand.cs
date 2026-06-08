using MediatR;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Commands.IssueTemporaryScopeToken;

public sealed record IssueTemporaryScopeTokenCommand(
    string SubjectId,
    string ResourceType,
    Guid ResourceId,
    IReadOnlyList<string> Permissions,
    TimeSpan Ttl,
    string? Reason = null) : IRequest<IssueTemporaryScopeTokenResult>;

public sealed record IssueTemporaryScopeTokenResult(string RawToken, DateTimeOffset ExpiresAt);
