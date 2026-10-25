using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Example03.Presentation.Authentication;

namespace Example03.Tests.UnitTests;

public class JwtAsyncActionSecurityFilterTests
{
    [Fact]
    public async Task When_Bearer_Token_Is_Valid_Then_Should_Call_Next()
    {
        // Arrange
        var filter = new JwtAsyncActionSecurityFilter(TestTokens.CreateValidator());

        // Act
        var (context, nextCalled) = await ExecuteAsync(filter, $"Bearer {TestTokens.Create()}");

        // Assert
        nextCalled.Should().BeTrue();
        context.Result.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Basic dXNlcjpwYXNz")]
    [InlineData("Bearer not-a-jwt")]
    public async Task When_Bearer_Token_Is_Missing_Or_Malformed_Then_Should_Returns_Unauthorized(string authorization)
    {
        // Arrange
        var filter = new JwtAsyncActionSecurityFilter(TestTokens.CreateValidator());

        // Act
        var (context, nextCalled) = await ExecuteAsync(filter, authorization);

        // Assert
        nextCalled.Should().BeFalse();
        context.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    private static async Task<(ActionExecutingContext Context, bool NextCalled)> ExecuteAsync(JwtAsyncActionSecurityFilter filter, string authorization)
    {
        var httpContext = new DefaultHttpContext();
        if (authorization is not null)
        {
            httpContext.Request.Headers[JwtConstants.AuthorizationHeaderName] = authorization;
        }

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var context = new ActionExecutingContext(actionContext, [], new Dictionary<string, object>(), new object());

        var nextCalled = false;
        await filter.OnActionExecutionAsync(context, () =>
        {
            nextCalled = true;
            return Task.FromResult<ActionExecutedContext>(null!);
        });

        return (context, nextCalled);
    }
}
