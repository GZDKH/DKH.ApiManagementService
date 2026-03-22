using DKH.ApiManagementService.Domain.Enums;
using DKH.Platform.Domain.Events;

namespace DKH.ApiManagementService.Domain.Entities.Events;

public sealed record AiProviderCreatedDomainEvent(
    Guid ProviderId,
    string Name,
    AiProviderType ProviderType) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}
