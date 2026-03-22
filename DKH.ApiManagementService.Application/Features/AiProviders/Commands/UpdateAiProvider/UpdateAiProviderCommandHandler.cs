using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Application.Features.AiProviders.Mappers;
using DKH.ApiManagementService.Contracts.ApiManagement.Models.AiProvider.v1;
using Grpc.Core;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.AiProviders.Commands.UpdateAiProvider;

public sealed class UpdateAiProviderCommandHandler(
    IAiProviderRepository repository) : IRequestHandler<UpdateAiProviderCommand, AiProviderModel>
{
    public async Task<AiProviderModel> Handle(UpdateAiProviderCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound,
                $"AI provider with id '{request.Id}' not found."));

        entity.Update(
            request.Name,
            request.DisplayName,
            request.BaseUrl,
            request.Models,
            request.ApiKeyReference,
            request.RateLimitPerMinute,
            request.DailyQuota);

        await repository.UpdateAsync(entity, cancellationToken);

        return entity.ToProto();
    }
}
