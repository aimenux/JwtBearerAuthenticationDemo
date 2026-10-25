using Microsoft.AspNetCore.Http;
using AwesomeAssertions;
using Example02.Presentation.Authentication;

namespace Example02.Tests.UnitTests;

public class JwtMiddlewareTests
{
    [Fact]
    public async Task When_Bearer_Token_Is_Valid_Then_Should_Returns_Ok()
    {
        // Arrange
        var context = CreateContext($"Bearer {TestTokens.Create()}");
        var middleware = new JwtMiddleware(_ => Task.CompletedTask, TestTokens.CreateValidator());

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        context.User.Identity!.IsAuthenticated.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Basic dXNlcjpwYXNz")]
    [InlineData("Bearer")]
    [InlineData("Bearer not-a-jwt")]
    public async Task When_Bearer_Token_Is_Missing_Or_Malformed_Then_Should_Returns_Unauthorized(string authorization)
    {
        // Arrange
        var context = CreateContext(authorization);
        var middleware = new JwtMiddleware(_ => Task.CompletedTask, TestTokens.CreateValidator());

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        context.Response.Headers.WWWAuthenticate.ToString().Should().Be(JwtConstants.BearerScheme);
    }

    [Fact]
    public async Task When_Bearer_Token_Is_Signed_With_Another_Key_Then_Should_Returns_Unauthorized()
    {
        // Arrange
        var context = CreateContext($"Bearer {TestTokens.Create(signingKey: "AnotherSigningKey4Tests-0123456789")}");
        var middleware = new JwtMiddleware(_ => Task.CompletedTask, TestTokens.CreateValidator());

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    private static DefaultHttpContext CreateContext(string authorization)
    {
        var context = new DefaultHttpContext
        {
            Response =
            {
                Body = new MemoryStream()
            }
        };

        if (authorization is not null)
        {
            context.Request.Headers[JwtConstants.AuthorizationHeaderName] = authorization;
        }

        return context;
    }
}
