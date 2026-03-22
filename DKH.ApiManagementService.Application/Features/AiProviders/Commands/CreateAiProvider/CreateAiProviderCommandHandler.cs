using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Application.Features.AiProviders.Mappers;
using DKH.ApiManagementService.Contracts.ApiManagement.Models.AiProvider.v1;
using DKH.ApiManagementService.Domain.Entities;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.AiProviders.Commands.CreateAiProvider;

public sealed class CreateAiProviderCommandHandler(
    IAiProviderRepository repository) : IRequestHandler<CreateAiProviderCommand, AiProviderModel>
{
    public async Task<AiProviderModel> Handle(CreateAiProviderCommand request, CancellationToken cancellationToken)
    {
        var entity = AiProviderEntity.Create(
            request.Name,
            request.ProviderType,
            request.DisplayName,
            request.BaseUrl,
            request.Models,
            request.ApiKeyReference,
            request.RateLimitPerMinute,
            request.DailyQuota);

        await repository.AddAsync(entity, cancellationToken);

        return entity.ToProto();
    }
}
