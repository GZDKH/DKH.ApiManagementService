using DKH.Platform.Domain.Events;

namespace DKH.ApiManagementService.Domain.Entities.Events;

public sealed record ApiKeyRegeneratedDomainEvent(
    Guid ApiKeyId,
    string Name) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}
