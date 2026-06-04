using DKH.Platform.Http.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DKH.ApiManagementService.Api.Controllers.DeveloperPortal.V1;

[ApiController]
[AllowAnonymous]
[ApiExplorerSettings(GroupName = "api-management")]
[Route("api/v1/developer-portal")]
public sealed class DeveloperPortalController(IOptions<PlatformOpenApiOptions> openApiOptions) : ControllerBase
{
    [HttpGet("documents")]
    [ProducesResponseType(typeof(DeveloperPortalDocumentsResponse), StatusCodes.Status200OK)]
    public ActionResult<DeveloperPortalDocumentsResponse> ListDocuments()
    {
        var options = openApiOptions.Value;
        var documents = options.Documents is { Count: > 0 }
            ? options.Documents
            : [PlatformOpenApiDocumentOptions.Default];

        return new DeveloperPortalDocumentsResponse(
            [
                .. documents
                .Select(document =>
                {
                    var name = string.IsNullOrWhiteSpace(document.Name)
                        ? PlatformOpenApiOptions.DefaultDocumentName
                        : document.Name;

                    return new DeveloperPortalDocumentDto(
                        name,
                        document.Title ?? name,
                        BuildDocumentRoute(options.JsonRoutePattern, name),
                        NormalizePrefix(options.ScalarUi.EndpointPrefix, "/scalar"),
                        NormalizePrefix(options.SwaggerUi.EndpointPrefix, "/swagger"));
                }),
            ]);
    }

    private static string BuildDocumentRoute(string? pattern, string documentName)
    {
        var routePattern = string.IsNullOrWhiteSpace(pattern) ? "/openapi/{documentName}.json" : pattern;
        var route = routePattern.Replace("{documentName}", documentName, StringComparison.OrdinalIgnoreCase);
        return NormalizePrefix(route, "/openapi/" + documentName + ".json");
    }

    private static string NormalizePrefix(string? candidate, string fallback)
    {
        var prefix = string.IsNullOrWhiteSpace(candidate) ? fallback : candidate;
        return prefix.StartsWith('/') ? prefix : "/" + prefix;
    }
}

public sealed record DeveloperPortalDocumentsResponse(IReadOnlyCollection<DeveloperPortalDocumentDto> Documents);

public sealed record DeveloperPortalDocumentDto(
    string Name,
    string Title,
    string OpenApiUrl,
    string ScalarUrl,
    string SwaggerUrl);
