using DKH.ApiManagementService.Domain.Entities;
using DKH.Platform.Grpc.Common.Types;
using Google.Protobuf.WellKnownTypes;
using ContractsApiKey = DKH.ApiManagementService.Contracts.ApiManagement.Models.ApiKey.v1.ApiKeyModel;
using ContractsEnvironment = DKH.ApiManagementService.Contracts.ApiManagement.Models.ApiKey.v1.ApiKeyEnvironment;
using ContractsRateLimitTier = DKH.ApiManagementService.Contracts.ApiManagement.Models.ApiKey.v1.ApiKeyRateLimitTier;
using ContractsScope = DKH.ApiManagementService.Contracts.ApiManagement.Models.ApiKey.v1.ApiKeyScope;
using ContractsStatus = DKH.ApiManagementService.Contracts.ApiManagement.Models.ApiKey.v1.ApiKeyStatus;
using DomainEnvironment = DKH.ApiManagementService.Domain.Enums.ApiKeyEnvironment;
using DomainRateLimitTier = DKH.ApiManagementService.Domain.Enums.ApiKeyRateLimitTier;
using DomainScope = DKH.ApiManagementService.Domain.Enums.ApiKeyScope;
using DomainStatus = DKH.ApiManagementService.Domain.Enums.ApiKeyStatus;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Mappers;

public static class ApiKeyMapper
{
    public static ContractsApiKey ToProto(this ApiKeyEntity entity)
    {
        var proto = new ContractsApiKey
        {
            Id = GuidValue.FromGuid(entity.Id),
            Name = entity.Name,
            KeyPrefix = entity.KeyPrefix,
            Scope = entity.Scope.ToProtoScope(),
            Status = entity.Status.ToProtoStatus(),
            Environment = entity.Environment.ToProtoEnvironment(),
            RateLimitTier = entity.RateLimitTier.ToProtoRateLimitTier(),
            RateLimitRequestsPerMinute = entity.RateLimitRequestsPerMinute,
            RotationCount = entity.RotationCount,
            CreatedAt = Timestamp.FromDateTime(DateTime.SpecifyKind(entity.CreationTime, DateTimeKind.Utc)),
            UpdatedAt = Timestamp.FromDateTime(DateTime.SpecifyKind(entity.LastModificationTime ?? entity.CreationTime, DateTimeKind.Utc)),
            CreatedBy = entity.CreatorId.HasValue ? GuidValue.FromGuid(entity.CreatorId.Value) : null,
        };

        proto.Permissions.AddRange(entity.Permissions);

        if (entity.Description is not null)
        {
            proto.Description = entity.Description;
        }

        if (entity.ExpiresAt.HasValue)
        {
            proto.ExpiresAt = Timestamp.FromDateTimeOffset(entity.ExpiresAt.Value);
        }

        if (entity.LastUsedAt.HasValue)
        {
            proto.LastUsedAt = Timestamp.FromDateTimeOffset(entity.LastUsedAt.Value);
        }

        if (entity.CustomerId.HasValue)
        {
            proto.CustomerId = GuidValue.FromGuid(entity.CustomerId.Value);
        }

        if (entity.LastRotatedAt.HasValue)
        {
            proto.LastRotatedAt = Timestamp.FromDateTimeOffset(entity.LastRotatedAt.Value);
        }

        if (entity.PreviousKeyPrefix is not null)
        {
            proto.PreviousKeyPrefix = entity.PreviousKeyPrefix;
        }

        return proto;
    }

    public static ContractsScope ToProtoScope(this DomainScope scope)
    {
        return scope switch
        {
            DomainScope.Mcp => ContractsScope.Mcp,
            DomainScope.Webhook => ContractsScope.Webhook,
            DomainScope.Partner => ContractsScope.Partner,
            DomainScope.Storefront => ContractsScope.Storefront,
            DomainScope.Internal => ContractsScope.Internal,
            _ => ContractsScope.Unspecified,
        };
    }

    public static DomainScope ToDomainScope(this ContractsScope scope)
    {
        return scope switch
        {
            ContractsScope.Mcp => DomainScope.Mcp,
            ContractsScope.Webhook => DomainScope.Webhook,
            ContractsScope.Partner => DomainScope.Partner,
            ContractsScope.Storefront => DomainScope.Storefront,
            ContractsScope.Internal => DomainScope.Internal,
            _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, "Unknown API key scope"),
        };
    }

    public static ContractsStatus ToProtoStatus(this DomainStatus status)
    {
        return status switch
        {
            DomainStatus.Active => ContractsStatus.Active,
            DomainStatus.Revoked => ContractsStatus.Revoked,
            DomainStatus.Expired => ContractsStatus.Expired,
            _ => ContractsStatus.Unspecified,
        };
    }

    public static ContractsEnvironment ToProtoEnvironment(this DomainEnvironment environment)
    {
        return environment switch
        {
            DomainEnvironment.Sandbox => ContractsEnvironment.Sandbox,
            DomainEnvironment.Production => ContractsEnvironment.Production,
            _ => ContractsEnvironment.Unspecified,
        };
    }

    public static DomainEnvironment ToDomainEnvironment(this ContractsEnvironment environment)
    {
        return environment switch
        {
            ContractsEnvironment.Sandbox => DomainEnvironment.Sandbox,
            ContractsEnvironment.Production => DomainEnvironment.Production,
            _ => throw new ArgumentOutOfRangeException(nameof(environment), environment, "Unknown API key environment"),
        };
    }

    public static ContractsRateLimitTier ToProtoRateLimitTier(this DomainRateLimitTier tier)
    {
        return tier switch
        {
            DomainRateLimitTier.Development => ContractsRateLimitTier.Development,
            DomainRateLimitTier.Standard => ContractsRateLimitTier.Standard,
            DomainRateLimitTier.Professional => ContractsRateLimitTier.Professional,
            DomainRateLimitTier.Enterprise => ContractsRateLimitTier.Enterprise,
            _ => ContractsRateLimitTier.Unspecified,
        };
    }

    public static DomainRateLimitTier ToDomainRateLimitTier(this ContractsRateLimitTier tier)
    {
        return tier switch
        {
            ContractsRateLimitTier.Development => DomainRateLimitTier.Development,
            ContractsRateLimitTier.Standard => DomainRateLimitTier.Standard,
            ContractsRateLimitTier.Professional => DomainRateLimitTier.Professional,
            ContractsRateLimitTier.Enterprise => DomainRateLimitTier.Enterprise,
            _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Unknown API key rate-limit tier"),
        };
    }
}
