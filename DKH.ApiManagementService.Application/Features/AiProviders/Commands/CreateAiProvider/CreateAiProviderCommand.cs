using DKH.ApiManagementService.Contracts.ApiManagement.Models.AiProvider.v1;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.AiProviders.Commands.CreateAiProvider;

public sealed record CreateAiProviderCommand(
    string Name,
    Domain.Enums.AiProviderType ProviderType,
    string? DisplayName = null,
    string? BaseUrl = null,
    List<string>? Models = null,
    string? ApiKeyReference = null,
    int? RateLimitPerMinute = null,
    int? DailyQuota = null) : IRequest<AiProviderModel>;
