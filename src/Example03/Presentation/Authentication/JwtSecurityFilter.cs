using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Example03.Presentation.Authentication;

public class JwtSecurityFilter : IAsyncAuthorizationFilter
{
    private readonly JwtTokenValidator _validator;

    public JwtSecurityFilter(JwtTokenValidator validator)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var principal = await _validator.AuthenticateAsync(context.HttpContext.Request.Headers);
        if (principal is null)
        {
            context.HttpContext.Response.Headers.WWWAuthenticate = JwtConstants.BearerScheme;
            context.Result = new UnauthorizedObjectResult("Bearer token is missing or invalid");
            return;
        }

        context.HttpContext.User = principal;
    }
}
