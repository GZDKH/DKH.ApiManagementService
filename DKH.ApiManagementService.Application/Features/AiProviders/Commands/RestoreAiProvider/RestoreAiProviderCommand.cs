using DKH.ApiManagementService.Contracts.ApiManagement.Models.AiProvider.v1;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.AiProviders.Commands.RestoreAiProvider;

public sealed record RestoreAiProviderCommand(Guid Id) : IRequest<AiProviderModel>;
