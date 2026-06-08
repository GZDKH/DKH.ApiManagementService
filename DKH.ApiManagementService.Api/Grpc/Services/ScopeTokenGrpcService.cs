using DKH.ApiManagementService.Application.Features.ApiKeys.Commands.IssueTemporaryScopeToken;
using DKH.ApiManagementService.Contracts.ApiManagement.Api.ScopeToken.v1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace DKH.ApiManagementService.Api.Grpc.Services;

[Authorize(Policy = ApiManagementServiceAuthorizationPolicies.ScopeTokenIssuerAccess)]
public sealed class ScopeTokenGrpcService(IMediator mediator) : ScopeTokenService.ScopeTokenServiceBase
{
    public override async Task<IssueTemporaryScopeTokenResponse> IssueTemporaryScopeToken(
        IssueTemporaryScopeTokenRequest request,
        ServerCallContext context)
    {
        var result = await mediator.Send(
            new IssueTemporaryScopeTokenCommand(
                request.SubjectId,
                request.ResourceType,
                request.ResourceId.ToGuid(),
                [.. request.Permissions],
                TimeSpan.FromSeconds(request.TtlSeconds),
                string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason),
            context.CancellationToken);

        return new IssueTemporaryScopeTokenResponse
        {
            RawToken = result.RawToken,
            ExpiresAt = Timestamp.FromDateTimeOffset(result.ExpiresAt),
        };
    }
}
