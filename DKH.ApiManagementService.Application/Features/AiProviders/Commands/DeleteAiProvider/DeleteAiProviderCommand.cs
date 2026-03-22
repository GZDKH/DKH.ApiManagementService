using MediatR;

namespace DKH.ApiManagementService.Application.Features.AiProviders.Commands.DeleteAiProvider;

public sealed record DeleteAiProviderCommand(Guid Id) : IRequest;
