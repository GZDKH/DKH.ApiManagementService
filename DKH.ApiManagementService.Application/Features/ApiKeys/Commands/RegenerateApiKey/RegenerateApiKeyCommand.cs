using DKH.ApiManagementService.Contracts.ApiManagement.Models.ApiKey.v1;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Commands.RegenerateApiKey;

public sealed record RegenerateApiKeyCommand(Guid Id) : IRequest<RegenerateApiKeyResult>;

public sealed record RegenerateApiKeyResult(ApiKeyModel ApiKey, string RawKey);
