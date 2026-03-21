using DKH.ApiManagementService.Contracts.ApiManagement.Models.ApiKey.v1;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Commands.RestoreApiKey;

public sealed record RestoreApiKeyCommand(Guid Id) : IRequest<ApiKeyModel>;
