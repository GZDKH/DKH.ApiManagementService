using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Domain.Entities;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.Usage.RecordUsage;

public sealed class RecordUsageCommandHandler(IApiKeyUsageRepository repository) : IRequestHandler<RecordUsageCommand, bool>
{
    public async Task<bool> Handle(RecordUsageCommand request, CancellationToken cancellationToken)
    {
        var usage = new ApiKeyUsageEntity(
            request.ApiKeyId,
            request.Endpoint,
            request.StatusCode,
            request.IpAddress,
            request.UserAgent,
            request.ResponseTimeMs);

        await repository.AddAsync(usage, cancellationToken);

        return true;
    }
}
