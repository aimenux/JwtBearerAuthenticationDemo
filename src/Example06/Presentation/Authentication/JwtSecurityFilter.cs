namespace Example06.Presentation.Authentication;

public class JwtSecurityFilter : IEndpointFilter
{
    private readonly JwtTokenValidator _validator;

    public JwtSecurityFilter(JwtTokenValidator validator)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    public async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var principal = await _validator.AuthenticateAsync(context.HttpContext.Request.Headers);
        if (principal is null)
        {
            return HttpResults.Unauthorized("Bearer token is missing or invalid");
        }

        context.HttpContext.User = principal;
        return await next(context);
    }
}
