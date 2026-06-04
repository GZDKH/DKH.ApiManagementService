using System.Text.Json;
using FluentAssertions;

namespace DKH.ApiManagementService.Tests.OpenApi;

public sealed class OpenApiPortalConfigurationTests
{
    [Fact]
    public void AppSettings_EnableDeveloperPortalOpenApiDocumentAndUi()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(GetAppSettingsPath()));

        var openApi = document.RootElement
            .GetProperty("Platform")
            .GetProperty("Http")
            .GetProperty("OpenApi");

        openApi.GetProperty("Enabled").GetBoolean().Should().BeTrue();
        openApi.GetProperty("JsonRoutePattern").GetString().Should().Be("/openapi/{documentName}.json");

        openApi.GetProperty("ScalarUi").GetProperty("Enabled").GetBoolean().Should().BeTrue();
        openApi.GetProperty("ScalarUi").GetProperty("EndpointPrefix").GetString().Should().Be("/scalar");

        openApi.GetProperty("SwaggerUi").GetProperty("Enabled").GetBoolean().Should().BeTrue();
        openApi.GetProperty("SwaggerUi").GetProperty("EndpointPrefix").GetString().Should().Be("/swagger");

        var documentConfig = openApi.GetProperty("Documents").EnumerateArray().Should().ContainSingle().Subject;
        documentConfig.GetProperty("Name").GetString().Should().Be("api-management");
        documentConfig.GetProperty("Title").GetString().Should().Be("DKH Public API Management");

        document.RootElement
            .GetProperty("Kestrel")
            .GetProperty("EndpointDefaults")
            .GetProperty("Protocols")
            .GetString()
            .Should()
            .Be("Http1AndHttp2");
    }

    [Fact]
    public void DevelopmentAppSettings_EnableHttp1AndHttp2OnNamedGrpcEndpoint()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(GetDevelopmentAppSettingsPath()));

        document.RootElement
            .GetProperty("Kestrel")
            .GetProperty("Endpoints")
            .GetProperty("Grpc")
            .GetProperty("Protocols")
            .GetString()
            .Should()
            .Be("Http1AndHttp2");
    }

    private static string GetAppSettingsPath()
        => Path.Combine(GetRepositoryRoot(), "DKH.ApiManagementService.Api", "appsettings.json");

    private static string GetDevelopmentAppSettingsPath()
        => Path.Combine(GetRepositoryRoot(), "DKH.ApiManagementService.Api", "appsettings.Development.json");

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "DKH.ApiManagementService.slnx")))
        {
            directory = directory.Parent;
        }

        directory.Should().NotBeNull("the test must run from inside the ApiManagementService repository");

        return directory!.FullName;
    }
}
