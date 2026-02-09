using MediatR;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Commands.DeleteApiKey;

public sealed record DeleteApiKeyCommand(Guid Id) : IRequest<bool>;
