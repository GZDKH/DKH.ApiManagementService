using MediatR;

namespace DKH.ApiManagementService.Application.Features.AiProviders.Commands.PermanentlyDeleteAiProvider;

public sealed record PermanentlyDeleteAiProviderCommand(Guid Id) : IRequest;
