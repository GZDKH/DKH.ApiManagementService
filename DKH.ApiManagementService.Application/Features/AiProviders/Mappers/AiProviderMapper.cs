using DKH.ApiManagementService.Domain.Entities;
using DKH.Platform.Grpc.Common.Types;
using Google.Protobuf.WellKnownTypes;
using ContractsAiProvider = DKH.ApiManagementService.Contracts.ApiManagement.Models.AiProvider.v1.AiProviderModel;
using ContractsProviderStatus = DKH.ApiManagementService.Contracts.ApiManagement.Models.AiProvider.v1.AiProviderStatus;
using ContractsProviderType = DKH.ApiManagementService.Contracts.ApiManagement.Models.AiProvider.v1.AiProviderType;
using DomainProviderStatus = DKH.ApiManagementService.Domain.Enums.AiProviderStatus;
using DomainProviderType = DKH.ApiManagementService.Domain.Enums.AiProviderType;

namespace DKH.ApiManagementService.Application.Features.AiProviders.Mappers;

public static class AiProviderMapper
{
    public static ContractsAiProvider ToProto(this AiProviderEntity entity)
    {
        var proto = new ContractsAiProvider
        {
            Id = GuidValue.FromGuid(entity.Id),
            Name = entity.Name,
            ProviderType = entity.ProviderType.ToProtoType(),
            Status = entity.Status.ToProtoStatus(),
            CreatedAt = Timestamp.FromDateTime(DateTime.SpecifyKind(entity.CreationTime, DateTimeKind.Utc)),
            UpdatedAt = Timestamp.FromDateTime(DateTime.SpecifyKind(entity.LastModificationTime ?? entity.CreationTime, DateTimeKind.Utc)),
            CreatedBy = entity.CreatorId.HasValue ? GuidValue.FromGuid(entity.CreatorId.Value) : null,
        };

        proto.Models.AddRange(entity.Models);

        if (entity.DisplayName is not null)
        {
            proto.DisplayName = entity.DisplayName;
        }

        if (entity.BaseUrl is not null)
        {
            proto.BaseUrl = entity.BaseUrl;
        }

        if (entity.ApiKeyReference is not null)
        {
            proto.ApiKeyReference = entity.ApiKeyReference;
        }

        if (entity.RateLimitPerMinute.HasValue)
        {
            proto.RateLimitPerMinute = entity.RateLimitPerMinute.Value;
        }

        if (entity.DailyQuota.HasValue)
        {
            proto.DailyQuota = entity.DailyQuota.Value;
        }

        return proto;
    }

    public static ContractsProviderType ToProtoType(this DomainProviderType providerType)
    {
        return providerType switch
        {
            DomainProviderType.Anthropic => ContractsProviderType.Anthropic,
            DomainProviderType.OpenAi => ContractsProviderType.OpenAi,
            DomainProviderType.GoogleGemini => ContractsProviderType.GoogleGemini,
            DomainProviderType.Azure => ContractsProviderType.Azure,
            DomainProviderType.Custom => ContractsProviderType.Custom,
            _ => ContractsProviderType.Unspecified,
        };
    }

    public static DomainProviderType ToDomainType(this ContractsProviderType providerType)
    {
        return providerType switch
        {
            ContractsProviderType.Anthropic => DomainProviderType.Anthropic,
            ContractsProviderType.OpenAi => DomainProviderType.OpenAi,
            ContractsProviderType.GoogleGemini => DomainProviderType.GoogleGemini,
            ContractsProviderType.Azure => DomainProviderType.Azure,
            ContractsProviderType.Custom => DomainProviderType.Custom,
            _ => throw new ArgumentOutOfRangeException(nameof(providerType), providerType, "Unknown AI provider type"),
        };
    }

    public static ContractsProviderStatus ToProtoStatus(this DomainProviderStatus status)
    {
        return status switch
        {
            DomainProviderStatus.Active => ContractsProviderStatus.Active,
            DomainProviderStatus.Inactive => ContractsProviderStatus.Inactive,
            DomainProviderStatus.Error => ContractsProviderStatus.Error,
            _ => ContractsProviderStatus.Unspecified,
        };
    }
}
