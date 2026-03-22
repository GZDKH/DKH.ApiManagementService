using DKH.ApiManagementService.Application.Abstractions;
using Grpc.Core;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.AiProviders.Commands.DeleteAiProvider;

public sealed class DeleteAiProviderCommandHandler(
    IAiProviderRepository repository) : IRequestHandler<DeleteAiProviderCommand>
{
    public async Task Handle(DeleteAiProviderCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound,
                $"AI provider with id '{request.Id}' not found."));

        await repository.DeleteAsync(entity, cancellationToken);
    }
}
