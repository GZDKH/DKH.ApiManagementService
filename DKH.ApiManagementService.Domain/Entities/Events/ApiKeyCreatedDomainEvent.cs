using DKH.ApiManagementService.Domain.Enums;
using DKH.Platform.Domain.Events;

namespace DKH.ApiManagementService.Domain.Entities.Events;

public sealed record ApiKeyCreatedDomainEvent(
    Guid ApiKeyId,
    string Name,
    ApiKeyScope Scope) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}
