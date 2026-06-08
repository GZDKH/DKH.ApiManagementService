using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Application.Features.ApiKeys.Commands.IssueTemporaryScopeToken;
using DKH.ApiManagementService.Domain.Entities;
using DKH.ApiManagementService.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace DKH.ApiManagementService.Tests.ApiKeys;

public sealed class IssueTemporaryScopeTokenCommandHandlerTests
{
    [Fact]
    public async Task Handle_CreatesInternalApiKeyThatExpiresAfterRequestedTtlAsync()
    {
        var repository = Substitute.For<IApiKeyRepository>();
        var keyGenerator = Substitute.For<IApiKeyGenerator>();
        keyGenerator.Generate(ApiKeyScope.Internal)
            .Returns(("dkh_int_temp_scope_token", "hash", "dkh_int_temp_"));

        ApiKeyEntity? persistedEntity = null;
        repository
            .AddAsync(Arg.Do<ApiKeyEntity>(entity => persistedEntity = entity), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var issuedAt = DateTimeOffset.UtcNow;
        var handler = new IssueTemporaryScopeTokenCommandHandler(repository, keyGenerator);
        var command = new IssueTemporaryScopeTokenCommand(
            SubjectId: "provider-temp-42",
            ResourceType: "engagement_request",
            ResourceId: Guid.NewGuid(),
            Permissions: ["engagement.report.write"],
            Ttl: TimeSpan.FromMinutes(15),
            Reason: "Temporary provider report upload");

        var result = await handler.Handle(command, CancellationToken.None);

        result.RawToken.Should().Be("dkh_int_temp_scope_token");
        result.ExpiresAt.Should().BeCloseTo(issuedAt.AddMinutes(15), TimeSpan.FromSeconds(3));

        persistedEntity.Should().NotBeNull();
        persistedEntity!.Name.Should().Be("temp-scope:provider-temp-42");
        persistedEntity.Scope.Should().Be(ApiKeyScope.Internal);
        persistedEntity.Permissions.Should().BeEquivalentTo(["engagement.report.write"]);
        persistedEntity.ExpiresAt.Should().Be(result.ExpiresAt);
        persistedEntity.Description.Should().Contain("engagement_request");
        persistedEntity.Description.Should().Contain(command.ResourceId.ToString());
        persistedEntity.IsExpired().Should().BeFalse();
    }
}
