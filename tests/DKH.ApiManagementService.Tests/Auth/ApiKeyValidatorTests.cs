using System.Security.Claims;
using DKH.ApiManagementService.Api.Auth;
using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Domain.Entities;
using DKH.ApiManagementService.Domain.Enums;
using DKH.ApiManagementService.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace DKH.ApiManagementService.Tests.Auth;

public sealed class ApiKeyValidatorTests
{
    private readonly IApiKeyRepository _repository = Substitute.For<IApiKeyRepository>();
    private readonly IServiceScopeFactory _scopeFactory;

    public ApiKeyValidatorTests()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_repository);
        var provider = services.BuildServiceProvider();
        _scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
    }

    [Fact]
    public async Task ValidateAsync_EmptyKey_ReturnsInvalidAsync()
    {
        var validator = CreateValidator();

        var result = await validator.ValidateAsync(string.Empty);

        result.IsValid.Should().BeFalse();
        await _repository.DidNotReceive().GetByKeyHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ValidateAsync_WhitespaceKey_ReturnsInvalidAsync()
    {
        var validator = CreateValidator();

        var result = await validator.ValidateAsync("   ");

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_UnknownKey_ReturnsInvalidAsync()
    {
        _repository.GetByKeyHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((ApiKeyEntity?)null);

        var validator = CreateValidator();

        var result = await validator.ValidateAsync("dkh_mcp_unknown");

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_RevokedKey_ReturnsInvalidAsync()
    {
        const string rawKey = "dkh_mcp_revoked_key_aaaaaaaaaaaaaaa";
        var hash = ApiKeyHash.FromRawKey(rawKey).Value;

        var entity = ApiKeyEntity.Create(
            "revoked-key",
            hash,
            "dkh_mcp_revoked_",
            ApiKeyScope.Mcp,
            ["read"]);
        entity.Revoke();

        _repository.GetByKeyHashAsync(hash, Arg.Any<CancellationToken>()).Returns(entity);

        var validator = CreateValidator();

        var result = await validator.ValidateAsync(rawKey);

        result.IsValid.Should().BeFalse();
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<ApiKeyEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ValidateAsync_ExpiredKey_ReturnsInvalidAsync()
    {
        const string rawKey = "dkh_mcp_expired_key_aaaaaaaaaaaaaa";
        var hash = ApiKeyHash.FromRawKey(rawKey).Value;

        var entity = ApiKeyEntity.Create(
            "expired-key",
            hash,
            "dkh_mcp_expired_",
            ApiKeyScope.Mcp,
            ["read"],
            expiresAt: DateTimeOffset.UtcNow.AddMinutes(-5));

        _repository.GetByKeyHashAsync(hash, Arg.Any<CancellationToken>()).Returns(entity);

        var validator = CreateValidator();

        var result = await validator.ValidateAsync(rawKey);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_ActiveKey_ReturnsValidWithClaimsAndRecordsUsageAsync()
    {
        const string rawKey = "dkh_mcp_active_key_aaaaaaaaaaaaaaa";
        var hash = ApiKeyHash.FromRawKey(rawKey).Value;

        var entity = ApiKeyEntity.Create(
            "active-key",
            hash,
            "dkh_mcp_active_",
            ApiKeyScope.Mcp,
            ["read", "write"]);

        _repository.GetByKeyHashAsync(hash, Arg.Any<CancellationToken>()).Returns(entity);

        var validator = CreateValidator();

        var result = await validator.ValidateAsync(rawKey);

        result.IsValid.Should().BeTrue();
        result.ClientName.Should().Be("active-key");
        result.Claims.Should().NotBeNull();
        result.Claims!.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == "active-key");
        result.Claims!.Should().Contain(c => c.Type == ApiKeyValidator.ApiKeyIdClaimType && c.Value == entity.Id.ToString());
        result.Claims!.Should().Contain(c => c.Type == ApiKeyValidator.ApiKeyScopeClaimType && c.Value == ApiKeyScope.Mcp.ToString());
        result.Claims!
            .Where(c => c.Type == ApiKeyValidator.PermissionClaimType)
            .Select(c => c.Value)
            .Should().BeEquivalentTo(["read", "write"]);

        await _repository.Received(1).UpdateAsync(entity, Arg.Any<CancellationToken>());
        entity.LastUsedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ValidateAsync_HashMatchesValidationHandlerAsync()
    {
        // Sanity check: validator must hash with the same SHA-256 lowercase-hex
        // algorithm used by ApiKeyHash.FromRawKey / ApiKeyGenerator / ValidateApiKeyQueryHandler.
        const string rawKey = "dkh_mcp_test_key_1234567890abcdef";
        var expectedHash = ApiKeyHash.FromRawKey(rawKey).Value;

        string? observedHash = null;
        _repository
            .GetByKeyHashAsync(Arg.Do<string>(h => observedHash = h), Arg.Any<CancellationToken>())
            .Returns((ApiKeyEntity?)null);

        var validator = CreateValidator();

        await validator.ValidateAsync(rawKey);

        observedHash.Should().Be(expectedHash);
    }

    private ApiKeyValidator CreateValidator() => new(_scopeFactory, NullLogger<ApiKeyValidator>.Instance);
}
