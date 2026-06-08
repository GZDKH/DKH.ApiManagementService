using DKH.ApiManagementService.Api.Grpc.Services;
using DKH.ApiManagementService.Application.Features.ApiKeys.Commands.IssueTemporaryScopeToken;
using DKH.ApiManagementService.Contracts.ApiManagement.Api.ScopeToken.v1;
using DKH.Platform.Grpc.Common.Types;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;
using NSubstitute;

namespace DKH.ApiManagementService.Tests.ApiKeys;

public sealed class ScopeTokenGrpcServiceTests
{
    [Fact]
    public async Task IssueTemporaryScopeToken_MapsRequestToCommandAndReturnsIssuedTokenAsync()
    {
        var resourceId = Guid.NewGuid();
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(15);
        var mediator = Substitute.For<IMediator>();
        IssueTemporaryScopeTokenCommand? observedCommand = null;
        mediator.Send(
                Arg.Do<IssueTemporaryScopeTokenCommand>(command => observedCommand = command),
                Arg.Any<CancellationToken>())
            .Returns(new IssueTemporaryScopeTokenResult("raw-scope-token", expiresAt));
        var sut = new ScopeTokenGrpcService(mediator);
        var request = new IssueTemporaryScopeTokenRequest
        {
            SubjectId = "provider-42",
            ResourceType = "service_provider_profile",
            ResourceId = GuidValue.FromGuid(resourceId),
            TtlSeconds = 900,
            Reason = "provider-verification-approved",
        };
        request.Permissions.Add("engagement.provider");

        var response = await sut.IssueTemporaryScopeToken(request, new TestServerCallContext());

        observedCommand.Should().NotBeNull();
        observedCommand!.SubjectId.Should().Be("provider-42");
        observedCommand.ResourceType.Should().Be("service_provider_profile");
        observedCommand.ResourceId.Should().Be(resourceId);
        observedCommand.Permissions.Should().BeEquivalentTo(["engagement.provider"]);
        observedCommand.Ttl.Should().Be(TimeSpan.FromSeconds(900));
        observedCommand.Reason.Should().Be("provider-verification-approved");
        response.RawToken.Should().Be("raw-scope-token");
        response.ExpiresAt.Should().Be(Timestamp.FromDateTimeOffset(expiresAt));
    }

    private sealed class TestServerCallContext : ServerCallContext
    {
        protected override string MethodCore => string.Empty;
        protected override string HostCore => string.Empty;
        protected override string PeerCore => string.Empty;
        protected override DateTime DeadlineCore => DateTime.MaxValue;
        protected override Metadata RequestHeadersCore => [];
        protected override CancellationToken CancellationTokenCore => CancellationToken.None;
        protected override Metadata ResponseTrailersCore => [];
        protected override Status StatusCore { get; set; }
        protected override WriteOptions? WriteOptionsCore { get; set; }
        protected override AuthContext AuthContextCore => new(string.Empty, []);

        protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options)
            => throw new NotSupportedException();

        protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
            => Task.CompletedTask;
    }
}
