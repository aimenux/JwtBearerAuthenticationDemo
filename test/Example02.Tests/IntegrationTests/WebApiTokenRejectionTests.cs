using System.Net;
using System.Text.Json;
using System.Security.Cryptography;
using AwesomeAssertions;
using Example02.Tests.IntegrationTests.Fixtures;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Example02.Tests.IntegrationTests;

public class WebApiTokenRejectionTests
{
    [Theory]
    [InlineData("api/movies/1")]
    [InlineData("api/movies/list")]
    public async Task Should_Reject_Unsigned_Token_With_Alg_None(string route)
    {
        // Arrange
        var header = Base64UrlEncoder.Encode("{\"alg\":\"none\",\"typ\":\"JWT\"}");
        var token = $"{header}.{Base64UrlEncoder.Encode(Payload())}.";

        // Act
        var response = await SendAsync(route, $"Bearer {token}");

        // Assert
        AssertUnauthorized(response);
    }

    [Theory]
    [InlineData("api/movies/1")]
    [InlineData("api/movies/list")]
    public async Task Should_Reject_Token_With_Modified_Payload(string route)
    {
        // Arrange
        var original = TestTokens.Create().Split('.');
        var forged = $"{original[0]}.{Base64UrlEncoder.Encode(Payload("admin"))}.{original[2]}";

        // Act
        var response = await SendAsync(route, $"Bearer {forged}");

        // Assert
        AssertUnauthorized(response);
    }

    [Theory]
    [InlineData("api/movies/1")]
    [InlineData("api/movies/list")]
    public async Task Should_Reject_Token_With_Wrong_Issuer(string route)
    {
        // Arrange
        var token = TestTokens.Create(issuer: "SomeOtherIssuer");

        // Act
        var response = await SendAsync(route, $"Bearer {token}");

        // Assert
        AssertUnauthorized(response);
    }

    [Theory]
    [InlineData("api/movies/1")]
    [InlineData("api/movies/list")]
    public async Task Should_Reject_Non_Bearer_Authorization_Scheme(string route)
    {
        // Arrange
        var authorization = "Basic dGVzdHM6dGVzdHM=";

        // Act
        var response = await SendAsync(route, authorization);

        // Assert
        AssertUnauthorized(response);
    }

    [Theory]
    [InlineData("api/movies/1")]
    [InlineData("api/movies/list")]
    public async Task Should_Reject_Bearer_Scheme_Without_Token(string route)
    {
        // Arrange
        var authorization = "Bearer";

        // Act
        var response = await SendAsync(route, authorization);

        // Assert
        AssertUnauthorized(response);
    }

    [Theory]
    [InlineData("api/movies/1")]
    [InlineData("api/movies/list")]
    public async Task Should_Reject_Token_Signed_With_RS256(string route)
    {
        // Arrange
        using var rsa = RSA.Create(2048);
        var token = SignWith(new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256));

        // Act
        var response = await SendAsync(route, $"Bearer {token}");

        // Assert
        AssertUnauthorized(response);
    }


    private static string Payload(string subject = "tests") => JsonSerializer.Serialize(new
    {
        iss = TestTokens.Issuer,
        aud = TestTokens.Audience,
        sub = subject,
        exp = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds()
    });

    private static string SignWith(SigningCredentials credentials) =>
        new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = TestTokens.Issuer,
            Audience = TestTokens.Audience,
            Expires = DateTime.UtcNow.AddMinutes(10),
            SigningCredentials = credentials
        });

    private static async Task<HttpResponseMessage> SendAsync(string route, string authorization)
    {
        await using var fixture = new WebApiTestsFixture(authorization);
        var client = fixture.CreateClient();

        return await client.GetAsync(route);
    }

    private static void AssertUnauthorized(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Headers.WwwAuthenticate.Should().ContainSingle(challenge => challenge.Scheme == Example02.Presentation.Authentication.JwtConstants.BearerScheme);
    }
}
