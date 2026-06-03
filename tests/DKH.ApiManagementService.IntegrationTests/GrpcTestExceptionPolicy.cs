using DKH.Platform.Exceptions;

namespace DKH.ApiManagementService.IntegrationTests;

public sealed class GrpcTestExceptionPolicy : IPlatformExceptionDescriptorPolicy
{
    public bool TryMap(Exception exception, out PlatformExceptionDescriptor descriptor)
    {
        descriptor = null!;
        return false;
    }
}
