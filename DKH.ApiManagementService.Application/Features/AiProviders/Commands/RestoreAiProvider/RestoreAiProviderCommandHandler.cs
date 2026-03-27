using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Application.Features.AiProviders.Mappers;
using DKH.ApiManagementService.Contracts.ApiManagement.Models.AiProvider.v1;
using Grpc.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DKH.ApiManagementService.Application.Features.AiProviders.Commands.RestoreAiProvider;

public sealed class RestoreAiProviderCommandHandler(IAppDbContext dbContext)
    : IRequestHandler<RestoreAiProviderCommand, AiProviderModel>
{
    public async Task<AiProviderModel> Handle(RestoreAiProviderCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.AiProviders
                         .IgnoreQueryFilters()
                         .FirstOrDefaultAsync(x => x.Id == request.Id && x.IsDeleted, cancellationToken)
                     ?? throw new RpcException(new Status(StatusCode.NotFound,
                         $"Soft-deleted AI provider {request.Id} not found"));

        entity.Restore();

        await dbContext.SaveChangesAsync(cancellationToken);

        return entity.ToProto();
    }
}
