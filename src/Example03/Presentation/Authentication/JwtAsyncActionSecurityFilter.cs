using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Example03.Presentation.Authentication;

public class JwtAsyncActionSecurityFilter : IAsyncActionFilter
{
    private readonly JwtTokenValidator _validator;

    public JwtAsyncActionSecurityFilter(JwtTokenValidator validator)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var principal = await _validator.AuthenticateAsync(context.HttpContext.Request.Headers);
        if (principal is null)
        {
            context.HttpContext.Response.Headers.WWWAuthenticate = JwtConstants.BearerScheme;
            context.Result = new UnauthorizedObjectResult("Bearer token is missing or invalid");
            return;
        }

        context.HttpContext.User = principal;
        await next();
    }
}
