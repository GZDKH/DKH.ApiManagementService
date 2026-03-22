using DKH.ApiManagementService.Contracts.ApiManagement.Models.AiProvider.v1;
using MediatR;

namespace DKH.ApiManagementService.Application.Features.AiProviders.Commands.UpdateAiProvider;

public sealed record UpdateAiProviderCommand(
    Guid Id,
    string? Name = null,
    string? DisplayName = null,
    string? BaseUrl = null,
    List<string>? Models = null,
    string? ApiKeyReference = null,
    int? RateLimitPerMinute = null,
    int? DailyQuota = null) : IRequest<AiProviderModel>;
