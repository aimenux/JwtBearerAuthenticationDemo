using System.Net;
using AwesomeAssertions;
using Microsoft.Extensions.Options;
using Example10.Presentation.Authentication;
using Example10.Tests.IntegrationTests.Fixtures;

namespace Example10.Tests.IntegrationTests;

public class WebApiTests
{
    [Theory]
    [InlineData("api/movies/1")]
    [InlineData("api/movies/list")]
    public async Task Should_Get_Movies_Returns_Success(string route)
    {
        // Arrange
        var fixture = new WebApiTestsFixture();
        var client = fixture.CreateClient();

        // Act
        var response = await client.GetAsync(route);
        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseBody.Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData("api/movies/1")]
    [InlineData("api/movies/list")]
    public async Task Should_Get_Movies_Returns_Unauthorized_When_Token_Is_Signed_With_Another_Key(string route)
    {
        // Arrange
        var token = TestTokens.CreateSignedByAnotherKey();
        var fixture = new WebApiTestsFixture($"Bearer {token}");
        var client = fixture.CreateClient();

        // Act
        var response = await client.GetAsync(route);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Headers.WwwAuthenticate.Should().ContainSingle(challenge => challenge.Scheme == JwtConstants.BearerScheme);
    }

    [Theory]
    [InlineData("api/movies/1")]
    [InlineData("api/movies/list")]
    public async Task Should_Get_Movies_Returns_Unauthorized_When_Token_Is_Expired(string route)
    {
        // Arrange
        var token = TestTokens.Create(expires: DateTime.UtcNow.AddMinutes(-10));
        var fixture = new WebApiTestsFixture($"Bearer {token}");
        var client = fixture.CreateClient();

        // Act
        var response = await client.GetAsync(route);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Headers.WwwAuthenticate.Should().ContainSingle(challenge => challenge.Scheme == JwtConstants.BearerScheme);
    }

    [Theory]
    [InlineData("api/movies/1")]
    [InlineData("api/movies/list")]
    public async Task Should_Get_Movies_Returns_Unauthorized_When_Token_Audience_Is_Wrong(string route)
    {
        // Arrange
        var token = TestTokens.Create(audience: "SomeOtherApi");
        var fixture = new WebApiTestsFixture($"Bearer {token}");
        var client = fixture.CreateClient();

        // Act
        var response = await client.GetAsync(route);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Headers.WwwAuthenticate.Should().ContainSingle(challenge => challenge.Scheme == JwtConstants.BearerScheme);
    }

    [Theory]
    [InlineData("api/movies/1")]
    [InlineData("api/movies/list")]
    public async Task Should_Get_Movies_Returns_Unauthorized_When_Authorization_Header_Is_Missing(string route)
    {
        // Arrange
        var fixture = new WebApiTestsFixture();
        var client = fixture.CreateClient();
        client.DefaultRequestHeaders.Remove(JwtConstants.AuthorizationHeaderName);

        // Act
        var response = await client.GetAsync(route);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Headers.WwwAuthenticate.Should().ContainSingle(challenge => challenge.Scheme == JwtConstants.BearerScheme);
    }

    [Theory]
    [InlineData("Basic dXNlcjpwYXNz")]
    [InlineData("Bearer")]
    [InlineData("Bearer not-a-jwt")]
    public async Task Should_Get_Movies_Returns_Unauthorized_When_Authorization_Header_Is_Not_A_Valid_Bearer_Token(string authorization)
    {
        // Arrange
        var fixture = new WebApiTestsFixture(authorization);
        var client = fixture.CreateClient();

        // Act
        var response = await client.GetAsync("api/movies/list");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Headers.WwwAuthenticate.Should().ContainSingle(challenge => challenge.Scheme == JwtConstants.BearerScheme);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-pem")]
    public void Should_Fail_To_Start_When_Public_Key_Is_Missing_Or_Invalid(string publicKeyPem)
    {
        // Arrange
        var fixture = new EmptyPublicKeyWebApiTestsFixture(publicKeyPem);

        // Act
        var act = () => fixture.CreateClient();

        // Assert
        var exception = act.Should().Throw<Exception>().Which;
        (exception is OptionsValidationException or ObjectDisposedException).Should().BeTrue();
    }
}
