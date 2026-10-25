using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Example06.Presentation.Authentication;

namespace Example06.Tests.UnitTests;

public class JwtSecurityFilterTests
{
    [Fact]
    public async Task When_Bearer_Token_Is_Valid_Then_Should_Returns_Ok()
    {
        // Arrange
        var context = CreateContext($"Bearer {TestTokens.Create()}");
        var securityFilter = new JwtSecurityFilter(TestTokens.CreateValidator());

        // Act
        var result = await securityFilter.InvokeAsync(context, _ => new ValueTask<object>(Results.Ok()));

        // Assert
        result.Should().BeOfType<Ok>();
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
        var result = await securityFilter.InvokeAsync(context, _ => new ValueTask<object>());

        // Assert
        result.Should().BeOfType<HttpResults.UnauthorizedHttpResultWithResponseBody>();
    }

    private static FilterContext CreateContext(string authorization)
    {
        var httpContext = new DefaultHttpContext();
        if (authorization is not null)
        {
            httpContext.Request.Headers[JwtConstants.AuthorizationHeaderName] = authorization;
        }

        return new FilterContext(httpContext);
    }

    private class FilterContext : EndpointFilterInvocationContext
    {
        public FilterContext(HttpContext httpContext)
        {
            HttpContext = httpContext;
            Arguments = new List<object>();
        }

        public override T GetArgument<T>(int index)
        {
            return default;
        }

        public override HttpContext HttpContext { get; }
        public override IList<object> Arguments { get; }
    }
}
