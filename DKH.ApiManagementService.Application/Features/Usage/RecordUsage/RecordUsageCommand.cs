using MediatR;

namespace DKH.ApiManagementService.Application.Features.Usage.RecordUsage;

public sealed record RecordUsageCommand(
    Guid ApiKeyId,
    string Endpoint,
    int StatusCode,
    string? IpAddress,
    string? UserAgent,
    long ResponseTimeMs) : IRequest<bool>;
