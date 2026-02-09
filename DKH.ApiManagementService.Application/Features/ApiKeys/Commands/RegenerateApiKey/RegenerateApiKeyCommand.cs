using DKH.ApiManagementService.Contracts.Models.V1;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Commands.RegenerateApiKey;

public sealed record RegenerateApiKeyCommand(Guid Id) : IRequest<RegenerateApiKeyResult>;

public sealed record RegenerateApiKeyResult(ApiKey ApiKey, string RawKey);
