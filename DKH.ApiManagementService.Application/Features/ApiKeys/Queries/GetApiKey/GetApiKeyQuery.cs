using DKH.ApiManagementService.Contracts.ApiManagement.Models.ApiKey.v1;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Queries.GetApiKey;

public sealed record GetApiKeyQuery(Guid Id) : IRequest<ApiKeyModel>;
