using System.Security.Claims;
using System.Security.Cryptography;
using Example09.Presentation.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Example09.Tests;

internal static class TestTokens
{
    public const string Issuer = "JwtAuthenticationDemo";
    public const string Audience = "MoviesApi";

    private static readonly RSA SigningRsaKey = RSA.Create(2048);

    public static string PublicKeyPem => SigningRsaKey.ExportRSAPublicKeyPem();

    public static string Create(
        string issuer = Issuer,
        string audience = Audience,
        DateTime? expires = null)
    {
        return CreateSignedWith(SigningRsaKey, issuer, audience, expires);
    }

    public static string CreateSignedByAnotherKey()
    {
        using var anotherKey = RSA.Create(2048);
        return CreateSignedWith(anotherKey, Issuer, Audience, null);
    }

    public static JwtTokenValidator CreateValidator()
    {
        return new(Options.Create(new JwtOptions { Issuer = Issuer, Audience = Audience, PublicKeyPem = PublicKeyPem }));
    }

    private static string CreateSignedWith(RSA key, string issuer, string audience, DateTime? expires)
    {
        var expiresAt = expires ?? DateTime.UtcNow.AddMinutes(10);
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Audience = audience,
            Subject = new ClaimsIdentity([new Claim(ClaimTypes.Name, "tests")]),
            NotBefore = expiresAt.AddHours(-1),
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(new RsaSecurityKey(key), SecurityAlgorithms.RsaSha256)
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
