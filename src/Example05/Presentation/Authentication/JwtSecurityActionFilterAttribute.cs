using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Example05.Presentation.Authentication;

public class JwtSecurityActionFilterAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetRequiredService<JwtTokenValidator>();
        var principal = await validator.AuthenticateAsync(context.HttpContext.Request.Headers);
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
