using DKH.ApiManagementService.Contracts.Models.V1;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.ApiKeys.Queries.GetApiKey;

public sealed record GetApiKeyQuery(Guid Id) : IRequest<ApiKey>;
