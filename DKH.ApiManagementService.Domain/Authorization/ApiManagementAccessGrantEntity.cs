using DKH.Platform.Authorization.ResourceAccess;
using DKH.Platform.Authorization.ResourceAccess.Domain;

namespace DKH.ApiManagementService.Domain.Authorization;

public sealed class ApiManagementAccessGrantEntity : ResourceAccessGrantEntity<Guid>
{
    private static readonly HashSet<string> AllowedResourceTypes = new(StringComparer.Ordinal) { "api_key" };

    private ApiManagementAccessGrantEntity() { }

    public ApiManagementAccessGrantEntity(
        Guid id, string resourceType, Guid resourceId,
        ResourceAccessSubjectType subjectType, string subjectId,
        ResourceAccessPermissions permissions, DateTime? expiresAt, string? grantReason)
        : base(id, resourceType, resourceId, subjectType, subjectId, permissions, expiresAt, grantReason)
    {
        if (!AllowedResourceTypes.Contains(resourceType))
        {
            throw new ArgumentException($"Resource type '{resourceType}' is not managed by ApiManagementService.", nameof(resourceType));
        }
    }
}
