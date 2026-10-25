using System.Net;
using System.Text.Json;
using AwesomeAssertions;
using Example03.Tests.IntegrationTests.Fixtures;

namespace Example03.Tests.IntegrationTests;

public class WebApiRouteTests
{
    [Fact]
    public async Task Should_Get_Movie_By_Id_Returns_Matching_Movie()
    {
        // Arrange
        await using var fixture = new WebApiTestsFixture();
        var client = fixture.CreateClient();

        // Act
        var response = await client.GetAsync("api/movies/1");
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var document = JsonDocument.Parse(body);
        document.RootElement.GetProperty("id").GetInt32().Should().Be(1);
        document.RootElement.GetProperty("title").GetString().Should().Be("Payback");
    }

    [Fact]
    public async Task Should_Get_Movies_List_Returns_All_Movies()
    {
        // Arrange
        await using var fixture = new WebApiTestsFixture();
        var client = fixture.CreateClient();

        // Act
        var response = await client.GetAsync("api/movies/list");
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var document = JsonDocument.Parse(body);
        document.RootElement.EnumerateArray().Select(movie => movie.GetProperty("title").GetString())
            .Should().BeEquivalentTo(["Payback", "Fearless"]);
    }

    [Fact]
    public async Task Should_Return_NotFound_For_Unknown_Movie()
    {
        // Arrange
        await using var fixture = new WebApiTestsFixture();
        var client = fixture.CreateClient();

        // Act
        var response = await client.GetAsync("api/movies/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_Return_NotFound_For_Non_Integer_Movie_Id()
    {
        // Arrange
        await using var fixture = new WebApiTestsFixture();
        var client = fixture.CreateClient();

        // Act
        var response = await client.GetAsync("api/movies/abc");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
