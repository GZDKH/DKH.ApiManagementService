using DKH.ApiManagementService.Application.Abstractions;
using Grpc.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DKH.ApiManagementService.Application.Features.AiProviders.Commands.PermanentlyDeleteAiProvider;

public sealed class PermanentlyDeleteAiProviderCommandHandler(IAppDbContext dbContext)
    : IRequestHandler<PermanentlyDeleteAiProviderCommand>
{
    public async Task Handle(PermanentlyDeleteAiProviderCommand request, CancellationToken cancellationToken)
    {
        var exists = await dbContext.AiProviders
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id == request.Id && x.IsDeleted, cancellationToken);

        if (!exists)
        {
            throw new RpcException(new Status(StatusCode.NotFound,
                $"Soft-deleted AI provider {request.Id} not found"));
        }

        await dbContext.AiProviders
            .IgnoreQueryFilters()
            .Where(x => x.Id == request.Id)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
