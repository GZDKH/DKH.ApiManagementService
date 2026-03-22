using DKH.ApiManagementService.Contracts.ApiManagement.Models.AiProvider.v1;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.AiProviders.Queries.GetAiProvider;

public sealed record GetAiProviderQuery(Guid Id) : IRequest<AiProviderModel>;
