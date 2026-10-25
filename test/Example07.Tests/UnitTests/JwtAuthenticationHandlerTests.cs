using System.Text.Encodings.Web;
using AwesomeAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Example07.Presentation.Authentication;

namespace Example07.Tests.UnitTests;

public class JwtAuthenticationHandlerTests
{
    [Fact]
    public async Task When_Bearer_Token_Is_Valid_Then_Should_Succeed()
    {
        // Arrange
        var handler = await CreateHandlerAsync($"Bearer {TestTokens.Create()}");

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Principal!.Identity!.IsAuthenticated.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Basic dXNlcjpwYXNz")]
    [InlineData("Bearer not-a-jwt")]
    public async Task When_Bearer_Token_Is_Missing_Or_Malformed_Then_Should_Fail(string authorization)
    {
        // Arrange
        var handler = await CreateHandlerAsync(authorization);

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failure!.Message.Should().Be("Bearer token is missing or invalid");
    }

    [Fact]
    public async Task When_Bearer_Token_Is_Signed_With_Another_Key_Then_Should_Fail()
    {
        // Arrange
        var handler = await CreateHandlerAsync($"Bearer {TestTokens.Create(signingKey: "AnotherSigningKey4Tests-0123456789")}");

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeFalse();
    }

    private static async Task<JwtAuthenticationHandler> CreateHandlerAsync(string authorization)
    {
        var httpContext = new DefaultHttpContext();
        if (authorization is not null)
        {
            httpContext.Request.Headers[JwtConstants.AuthorizationHeaderName] = authorization;
        }

        var optionsMonitor = Substitute.For<IOptionsMonitor<AuthenticationSchemeOptions>>();
        optionsMonitor.Get(Arg.Any<string>()).Returns(new AuthenticationSchemeOptions());

        var handler = new JwtAuthenticationHandler(optionsMonitor, NullLoggerFactory.Instance, UrlEncoder.Default, TestTokens.CreateValidator());
        var scheme = new AuthenticationScheme(JwtConstants.BearerScheme, null, typeof(JwtAuthenticationHandler));
        await handler.InitializeAsync(scheme, httpContext);
        return handler;
    }
}
