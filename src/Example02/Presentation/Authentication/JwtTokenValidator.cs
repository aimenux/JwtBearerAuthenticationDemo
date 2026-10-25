using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Example02.Presentation.Authentication;

public sealed class JwtTokenValidator
{
    private readonly JsonWebTokenHandler _tokenHandler = new();
    private readonly TokenValidationParameters _validationParameters;

    public JwtTokenValidator(IOptions<JwtOptions> options)
    {
        var jwt = options.Value;
        _validationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true
        };
    }

    public async Task<ClaimsPrincipal> AuthenticateAsync(IHeaderDictionary headers)
    {
        if (!AuthenticationHeaderValue.TryParse(headers.Authorization.ToString(), out var authorization)
            || !string.Equals(authorization.Scheme, JwtConstants.BearerScheme, StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(authorization.Parameter))
        {
            return null;
        }

        var result = await _tokenHandler.ValidateTokenAsync(authorization.Parameter, _validationParameters);
        return result.IsValid ? new ClaimsPrincipal(result.ClaimsIdentity) : null;
    }
}
