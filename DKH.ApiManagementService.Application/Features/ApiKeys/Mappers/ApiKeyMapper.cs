using DKH.ApiManagementService.Domain.Entities;
using Google.Protobuf.WellKnownTypes;
using ContractsApiKey = DKH.ApiManagementService.Contracts.Models.V1.ApiKey;
using ContractsScope = DKH.ApiManagementService.Contracts.Models.V1.ApiKeyScope;
using ContractsStatus = DKH.ApiManagementService.Contracts.Models.V1.ApiKeyStatus;
using DomainScope = DKH.ApiManagementService.Domain.Enums.ApiKeyScope;
using DomainStatus = DKH.ApiManagementService.Domain.Enums.ApiKeyStatus;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Mappers;

public static class ApiKeyMapper
{
    public static ContractsApiKey ToProto(this ApiKeyEntity entity)
    {
        var proto = new ContractsApiKey
        {
            Id = entity.Id.ToString(),
            Name = entity.Name,
            KeyPrefix = entity.KeyPrefix,
            Scope = entity.Scope.ToProtoScope(),
            Status = entity.Status.ToProtoStatus(),
            CreatedAt = Timestamp.FromDateTimeOffset(entity.CreatedAt),
            UpdatedAt = Timestamp.FromDateTimeOffset(entity.UpdatedAt),
            CreatedBy = entity.CreatedBy,
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
}
