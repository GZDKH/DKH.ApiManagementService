using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Application.Features.ApiKeys.Commands.CreateApiKey;
using DKH.ApiManagementService.Application.Features.Validation.ValidateApiKey;
using DKH.ApiManagementService.Domain.Entities;
using DKH.ApiManagementService.Domain.Enums;
using DKH.ApiManagementService.Domain.ValueObjects;
using DKH.ApiManagementService.Infrastructure.Persistence;
using DKH.ApiManagementService.Infrastructure.Persistence.Repositories;
using DKH.Platform.Identity;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace DKH.ApiManagementService.Tests.ApiKeys;

public sealed class StorefrontScopedApiKeyTests
{
    [Fact]
    public async Task CreateHandler_WithStorefrontId_PersistsItOnEntityAsync()
    {
        var storefrontId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var repository = Substitute.For<IApiKeyRepository>();
        var keyGenerator = Substitute.For<IApiKeyGenerator>();
        keyGenerator.Generate(ApiKeyScope.Storefront)
            .Returns(("dkh_sf_raw_key_aaaaaaaaaaaaaaaa", "hashabc", "dkh_sf_raw_"));

        ApiKeyEntity? persisted = null;
        repository
            .AddAsync(Arg.Do<ApiKeyEntity>(e => persisted = e), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var handler = new CreateApiKeyCommandHandler(repository, keyGenerator);
        var command = new CreateApiKeyCommand(
            Name: "storefront-public-key",
            Scope: ApiKeyScope.Storefront,
            Permissions: ["catalog.read"],
            Description: "Storefront public API key",
            ExpiresAt: null,
            CustomerId: customerId,
            StorefrontId: storefrontId,
            Environment: ApiKeyEnvironment.Production,
            RateLimitTier: ApiKeyRateLimitTier.Standard);

        var result = await handler.Handle(command, CancellationToken.None);

        result.RawKey.Should().Be("dkh_sf_raw_key_aaaaaaaaaaaaaaaa");
        persisted.Should().NotBeNull();
        persisted!.StorefrontId.Should().Be(storefrontId);
        persisted.CustomerId.Should().Be(customerId);
        persisted.Scope.Should().Be(ApiKeyScope.Storefront);
    }

    [Fact]
    public async Task ValidateHandler_RoundTrip_ReturnsStorefrontIdAsync()
    {
        var storefrontId = Guid.NewGuid();
        const string rawKey = "dkh_sf_validate_key_aaaaaaaaaaaaa";

        var services = new ServiceCollection();
        services.AddSingleton(Substitute.For<IPlatformCurrentUser>());
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase($"apikey-storefront-validate-{Guid.NewGuid()}"));

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var apiKey = ApiKeyEntity.Create(
            "storefront-validate-key",
            ApiKeyHash.FromRawKey(rawKey).Value,
            "dkh_sf_validate_",
            ApiKeyScope.Storefront,
            ["catalog.read"],
            customerId: null,
            storefrontId: storefrontId,
            ApiKeyEnvironment.Production,
            ApiKeyRateLimitTier.Standard);

        dbContext.ApiKeys.Add(apiKey);
        await dbContext.SaveChangesAsync();

        var handler = new ValidateApiKeyQueryHandler(new ApiKeyRepository(dbContext));
        var result = await handler.Handle(new ValidateApiKeyQuery(rawKey), CancellationToken.None);

        result.IsValid.Should().BeTrue();
        result.StorefrontId.Should().Be(storefrontId);
        result.Scope.Should().Be(ApiKeyScope.Storefront);
    }
}
