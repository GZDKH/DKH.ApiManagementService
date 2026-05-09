using DKH.ApiManagementService.Domain.Authorization;
using DKH.Platform.Authorization.ResourceAccess.EntityFrameworkCore.Configurations;

namespace DKH.ApiManagementService.Infrastructure.Persistence.Configurations;

internal sealed class ApiManagementAccessGrantConfiguration
    : ResourceAccessGrantConfigurationBase<ApiManagementAccessGrantEntity, Guid>
{
    protected override string TableName => "api_management_access_grants";
    protected override string? Schema => null;
}
