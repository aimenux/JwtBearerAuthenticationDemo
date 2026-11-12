using System.Net;
using System.Text.Json;
using AwesomeAssertions;
using Example02.Presentation.Authentication;
using Example02.Tests.IntegrationTests.Fixtures;

namespace Example02.Tests.IntegrationTests;

public class SwaggerTests
{
    private const string SwaggerDocumentRoute = "swagger/v1/swagger.json";

    [Fact]
    public async Task Should_Get_Swagger_Document_Without_Token()
    {
        // Arrange
        await using var fixture = new WebApiTestsFixture();
        var client = fixture.CreateClient();
        client.DefaultRequestHeaders.Remove(JwtConstants.AuthorizationHeaderName);

        // Act
        var response = await client.GetAsync(SwaggerDocumentRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task Should_Describe_Movies_Routes_In_Swagger_Document()
    {
        // Arrange
        await using var fixture = new WebApiTestsFixture();
        var client = fixture.CreateClient();

        // Act
        using var document = await GetSwaggerDocumentAsync(client);

        // Assert
        var paths = document.RootElement.GetProperty("paths");
        paths.TryGetProperty("/api/movies/list", out _).Should().BeTrue();
        paths.TryGetProperty("/api/movies/{movieId}", out _).Should().BeTrue();
    }

    [Fact]
    public async Task Should_Declare_Bearer_Security_Scheme_In_Swagger_Document()
    {
        // Arrange
        await using var fixture = new WebApiTestsFixture();
        var client = fixture.CreateClient();

        // Act
        using var document = await GetSwaggerDocumentAsync(client);

        // Assert
        var scheme = document.RootElement
            .GetProperty("components")
            .GetProperty("securitySchemes")
            .GetProperty(JwtConstants.BearerScheme);
        scheme.GetProperty("type").GetString().Should().Be("http");
        scheme.GetProperty("scheme").GetString().Should().BeEquivalentTo(JwtConstants.BearerScheme);
        scheme.GetProperty("bearerFormat").GetString().Should().Be("JWT");
    }

    [Fact]
    public async Task Should_Require_Bearer_Token_In_Swagger_Document()
    {
        // Arrange
        await using var fixture = new WebApiTestsFixture();
        var client = fixture.CreateClient();

        // Act
        using var document = await GetSwaggerDocumentAsync(client);

        // Assert
        var security = document.RootElement.GetProperty("security");
        security.EnumerateArray()
            .Any(requirement => requirement.TryGetProperty(JwtConstants.BearerScheme, out _))
            .Should().BeTrue();
    }

    private static async Task<JsonDocument> GetSwaggerDocumentAsync(HttpClient client)
    {
        var response = await client.GetAsync(SwaggerDocumentRoute);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }
}
