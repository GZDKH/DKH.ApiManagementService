using DKH.ApiManagementService.Application.Abstractions;
using Grpc.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DKH.ApiManagementService.Application.Features.AiProviders.Commands.DeleteAiProvider;

public sealed class DeleteAiProviderCommandHandler(
    IAppDbContext dbContext) : IRequestHandler<DeleteAiProviderCommand>
{
    public async Task Handle(DeleteAiProviderCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.AiProviders
                         .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
                     ?? throw new RpcException(new Status(StatusCode.NotFound,
                         $"AI provider with id '{request.Id}' not found."));

        entity.MarkAsDeleted();
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
