using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Example05.Presentation.Authentication;

namespace Example05.Tests.UnitTests;

public class JwtSecurityFilterTests
{
    [Fact]
    public async Task When_Bearer_Token_Is_Valid_Then_Should_Not_Short_Circuit()
    {
        // Arrange
        var context = CreateContext($"Bearer {TestTokens.Create()}");
        var securityFilter = new JwtSecurityFilter(TestTokens.CreateValidator());

        // Act
        await securityFilter.OnAuthorizationAsync(context);

        // Assert
        context.Result.Should().BeNull();
        context.HttpContext.User.Identity!.IsAuthenticated.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Basic dXNlcjpwYXNz")]
    [InlineData("Bearer not-a-jwt")]
    public async Task When_Bearer_Token_Is_Missing_Or_Malformed_Then_Should_Returns_Unauthorized(string authorization)
    {
        // Arrange
        var context = CreateContext(authorization);
        var securityFilter = new JwtSecurityFilter(TestTokens.CreateValidator());

        // Act
        await securityFilter.OnAuthorizationAsync(context);

        // Assert
        context.Result.Should().BeOfType<UnauthorizedObjectResult>();
        context.HttpContext.Response.Headers.WWWAuthenticate.ToString().Should().Be(JwtConstants.BearerScheme);
    }

    private static AuthorizationFilterContext CreateContext(string authorization)
    {
        var httpContext = new DefaultHttpContext();
        if (authorization is not null)
        {
            httpContext.Request.Headers[JwtConstants.AuthorizationHeaderName] = authorization;
        }

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new AuthorizationFilterContext(actionContext, []);
    }
}
