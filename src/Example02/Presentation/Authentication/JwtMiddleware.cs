namespace Example02.Presentation.Authentication;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly JwtTokenValidator _validator;

    public JwtMiddleware(RequestDelegate next, JwtTokenValidator validator)
    {
        _next = next;
        _validator = validator;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var principal = await _validator.AuthenticateAsync(context.Request.Headers);
        if (principal is null)
        {
            context.Response.Headers.WWWAuthenticate = JwtConstants.BearerScheme;
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Bearer token is missing or invalid");
            return;
        }

        context.User = principal;
        await _next(context);
    }
}

public static class AuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseJwtAuthentication(this IApplicationBuilder app)
    {
        app.UseMiddleware<JwtMiddleware>();
        return app;
    }
}
