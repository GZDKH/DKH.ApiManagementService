using DKH.ApiManagementService.Api.Grpc.Services;
using DKH.ApiManagementService.Application.Features.AiProviders.Queries.ListAiProviders;
using DKH.ApiManagementService.Contracts.ApiManagement.Api.AiProviderCrud.v1;
using DKH.ApiManagementService.Contracts.ApiManagement.Models.AiProvider.v1;
using DKH.Platform.Domain.Enums;
using FluentAssertions;
using Grpc.Core;
using MediatR;
using NSubstitute;

namespace DKH.ApiManagementService.Tests.AiProviders;

/// <summary>
///     ResolveActiveProvider is the internal service-to-service AI-key resolution path: it must stay
///     narrow (one active provider, runtime fields only) so opening it to internal callers never
///     exposes the admin listing surface.
/// </summary>
public sealed class AiProviderCrudGrpcServiceResolveTests
{
    [Fact]
    public async Task ResolveActiveProvider_QueriesOnlyTheSingleActiveProviderOfTheRequestedTypeAsync()
    {
        var mediator = Substitute.For<IMediator>();
        ListAiProvidersQuery? observedQuery = null;
        mediator.Send(
                Arg.Do<ListAiProvidersQuery>(query => observedQuery = query),
                Arg.Any<CancellationToken>())
            .Returns(new ListAiProvidersResult([BuildProvider()], 1));
        var sut = new AiProviderCrudGrpcService(mediator);

        await sut.ResolveActiveProvider(
            new ResolveActiveProviderRequest { ProviderType = AiProviderType.Anthropic },
            new TestServerCallContext());

        observedQuery.Should().NotBeNull();
        observedQuery!.TypeFilter.Should().Be(Domain.Enums.AiProviderType.Anthropic);
        observedQuery.StatusFilter.Should().Be(Domain.Enums.AiProviderStatus.Active);
        observedQuery.SoftDeleteFilter.Should().Be(PlatformSoftDeleteFilter.ActiveOnly);
        observedQuery.Page.Should().Be(1);
        observedQuery.PageSize.Should().Be(1);
    }

    [Fact]
    public async Task ResolveActiveProvider_ReturnsRuntimeFieldsOfTheActiveProviderAsync()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ListAiProvidersQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ListAiProvidersResult([BuildProvider()], 1));
        var sut = new AiProviderCrudGrpcService(mediator);

        var response = await sut.ResolveActiveProvider(
            new ResolveActiveProviderRequest { ProviderType = AiProviderType.Anthropic },
            new TestServerCallContext());

        response.Provider.Should().NotBeNull();
        response.Provider.ProviderType.Should().Be(AiProviderType.Anthropic);
        response.Provider.BaseUrl.Should().Be("https://api.anthropic.com");
        response.Provider.ApiKeyReference.Should().Be("sops://anthropic/api-key");
        response.Provider.Models.Should().BeEquivalentTo(["claude-opus-4-8"]);
    }

    [Fact]
    public async Task ResolveActiveProvider_ReturnsEmptyResponseWhenNoActiveProviderConfiguredAsync()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ListAiProvidersQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ListAiProvidersResult([], 0));
        var sut = new AiProviderCrudGrpcService(mediator);

        var response = await sut.ResolveActiveProvider(
            new ResolveActiveProviderRequest { ProviderType = AiProviderType.Anthropic },
            new TestServerCallContext());

        response.Provider.Should().BeNull();
    }

    [Fact]
    public async Task ResolveActiveProvider_RejectsUnspecifiedProviderTypeAsync()
    {
        var mediator = Substitute.For<IMediator>();
        var sut = new AiProviderCrudGrpcService(mediator);

        var act = async () => await sut.ResolveActiveProvider(
            new ResolveActiveProviderRequest { ProviderType = AiProviderType.Unspecified },
            new TestServerCallContext());

        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        await mediator.DidNotReceive().Send(Arg.Any<ListAiProvidersQuery>(), Arg.Any<CancellationToken>());
    }

    private static AiProviderModel BuildProvider()
    {
        var provider = new AiProviderModel
        {
            Name = "anthropic-primary",
            ProviderType = AiProviderType.Anthropic,
            Status = AiProviderStatus.Active,
            BaseUrl = "https://api.anthropic.com",
            ApiKeyReference = "sops://anthropic/api-key",
        };

        provider.Models.Add("claude-opus-4-8");
        return provider;
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
