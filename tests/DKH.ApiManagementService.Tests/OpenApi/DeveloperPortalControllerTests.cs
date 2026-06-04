using DKH.ApiManagementService.Api.Controllers.DeveloperPortal.V1;
using DKH.Platform.Http.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace DKH.ApiManagementService.Tests.OpenApi;

public sealed class DeveloperPortalControllerTests
{
    [Fact]
    public void ListDocuments_ReturnsConfiguredOpenApiAndUiRoutes()
    {
        var controller = new DeveloperPortalController(Options.Create(new PlatformOpenApiOptions
        {
            JsonRoutePattern = "/openapi/{documentName}.json",
            ScalarUi = new PlatformOpenApiScalarUiOptions
            {
                Enabled = true,
                EndpointPrefix = "/scalar",
            },
            SwaggerUi = new PlatformOpenApiSwaggerUiOptions
            {
                Enabled = true,
                EndpointPrefix = "/swagger",
            },
            Documents =
            [
                new PlatformOpenApiDocumentOptions
                {
                    Name = "api-management",
                    Title = "DKH Public API Management",
                },
            ],
        }));

        var response = controller.ListDocuments().Value;

        var document = response!.Documents.Should().ContainSingle().Subject;
        document.Name.Should().Be("api-management");
        document.Title.Should().Be("DKH Public API Management");
        document.OpenApiUrl.Should().Be("/openapi/api-management.json");
        document.ScalarUrl.Should().Be("/scalar");
        document.SwaggerUrl.Should().Be("/swagger");
    }
}
