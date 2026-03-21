using MediatR;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Commands.PermanentlyDeleteApiKey;

public sealed record PermanentlyDeleteApiKeyCommand(Guid Id) : IRequest;
