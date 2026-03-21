using DKH.ApiManagementService.Application.Abstractions;
using Grpc.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Commands.PermanentlyDeleteApiKey;

public sealed class PermanentlyDeleteApiKeyCommandHandler(IAppDbContext dbContext)
    : IRequestHandler<PermanentlyDeleteApiKeyCommand>
{
    public async Task Handle(PermanentlyDeleteApiKeyCommand request, CancellationToken cancellationToken)
    {
        var exists = await dbContext.ApiKeys
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id == request.Id && x.IsDeleted, cancellationToken);

        if (!exists)
        {
            throw new RpcException(new Status(StatusCode.NotFound,
                $"Soft-deleted API key {request.Id} not found"));
        }

        // Delete usage records first (child records)
        await dbContext.ApiKeyUsageRecords
            .Where(u => u.ApiKeyId == request.Id)
            .ExecuteDeleteAsync(cancellationToken);

        // Delete the API key (physical removal, bypasses soft-delete)
        await dbContext.ApiKeys
            .IgnoreQueryFilters()
            .Where(x => x.Id == request.Id)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
