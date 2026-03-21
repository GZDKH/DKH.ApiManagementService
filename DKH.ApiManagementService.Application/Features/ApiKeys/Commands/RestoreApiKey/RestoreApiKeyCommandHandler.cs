using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Application.Features.ApiKeys.Mappers;
using DKH.ApiManagementService.Contracts.ApiManagement.Models.ApiKey.v1;
using Grpc.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Commands.RestoreApiKey;

public sealed class RestoreApiKeyCommandHandler(IAppDbContext dbContext)
    : IRequestHandler<RestoreApiKeyCommand, ApiKeyModel>
{
    public async Task<ApiKeyModel> Handle(RestoreApiKeyCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ApiKeys
                         .IgnoreQueryFilters()
                         .FirstOrDefaultAsync(x => x.Id == request.Id && x.IsDeleted, cancellationToken)
                     ?? throw new RpcException(new Status(StatusCode.NotFound,
                         $"Soft-deleted API key {request.Id} not found"));

        entity.Restore();

        await dbContext.SaveChangesAsync(cancellationToken);

        return entity.ToProto();
    }
}
