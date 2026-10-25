using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Example06.Presentation.Authentication;

namespace Example06.Tests;

internal static class TestTokens
{
    public const string Issuer = "JwtAuthenticationDemo";
    public const string Audience = "MoviesApi";
    public const string SigningKey = "SigningKey4Tests-0123456789abcdef";

    public static string Create(
        string signingKey = SigningKey,
        string issuer = Issuer,
        string audience = Audience,
        DateTime? expires = null)
    {
        var expiresAt = expires ?? DateTime.UtcNow.AddMinutes(10);
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Audience = audience,
            Subject = new ClaimsIdentity([new Claim(ClaimTypes.Name, "tests")]),
            NotBefore = expiresAt.AddHours(-1),
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                SecurityAlgorithms.HmacSha256)
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    public static JwtTokenValidator CreateValidator()
    {
        return new(Options.Create(new JwtOptions { Issuer = Issuer, Audience = Audience, SigningKey = SigningKey }));
    }
}
