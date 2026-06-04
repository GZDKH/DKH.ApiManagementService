using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DKH.ApiManagementService.Application.Features.Usage.RecordUsage;

public sealed class RecordUsageCommandHandler(IApiKeyUsageRepository repository, IAppDbContext dbContext) : IRequestHandler<RecordUsageCommand, bool>
{
    public async Task<bool> Handle(RecordUsageCommand request, CancellationToken cancellationToken)
    {
        var apiKey = await dbContext.ApiKeys
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.ApiKeyId, cancellationToken);

        var usage = apiKey is null
            ? ApiKeyUsageEntity.Create(
                request.ApiKeyId,
                request.Endpoint,
                request.StatusCode,
                request.IpAddress,
                request.UserAgent,
                request.ResponseTimeMs)
            : ApiKeyUsageEntity.Create(
                request.ApiKeyId,
                request.Endpoint,
                request.StatusCode,
                request.IpAddress,
                request.UserAgent,
                request.ResponseTimeMs,
                apiKey.CustomerId,
                apiKey.Environment,
                apiKey.RateLimitTier,
                apiKey.RateLimitRequestsPerMinute);

        await repository.AddAsync(usage, cancellationToken);

        return true;
    }
}
