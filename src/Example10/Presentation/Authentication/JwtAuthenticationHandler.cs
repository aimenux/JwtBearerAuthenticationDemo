using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Example10.Presentation.Authentication;

public class JwtAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly JwtTokenValidator _validator;

    public JwtAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        JwtTokenValidator validator) : base(options, logger, encoder)
    {
        _validator = validator;
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.Headers.WWWAuthenticate = JwtConstants.BearerScheme;
        return base.HandleChallengeAsync(properties);
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var principal = await _validator.AuthenticateAsync(Request.Headers);
        if (principal is null)
        {
            return AuthenticateResult.Fail("Bearer token is missing or invalid");
        }

        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }
}
