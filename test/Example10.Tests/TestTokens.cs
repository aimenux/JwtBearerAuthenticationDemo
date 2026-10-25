using System.Security.Claims;
using System.Security.Cryptography;
using Example10.Presentation.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Example10.Tests;

internal static class TestTokens
{
    public const string Issuer = "JwtAuthenticationDemo";
    public const string Audience = "MoviesApi";

    private static readonly (RSAParameters Parameters, string PublicPem) SigningKey = CreateSigningKey();

    public static string PublicKeyPem => SigningKey.PublicPem;

    public static string Create(
        string issuer = Issuer,
        string audience = Audience,
        DateTime? expires = null)
    {
        return CreateSignedWith(SigningKey.Parameters, issuer, audience, expires);
    }

    public static string CreateSignedByAnotherKey()
    {
        using var anotherKey = RSA.Create(2048);
        return CreateSignedWith(anotherKey.ExportParameters(true), Issuer, Audience, null);
    }

    public static JwtTokenValidator CreateValidator()
    {
        return new JwtTokenValidator(Options.Create(new JwtOptions { Issuer = Issuer, Audience = Audience, PublicKeyPem = PublicKeyPem }));
    }

    private static string CreateSignedWith(RSAParameters parameters, string issuer, string audience, DateTime? expires)
    {
        using var key = RSA.Create();
        key.ImportParameters(parameters);
        var cryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false };
        var securityKey = new RsaSecurityKey(key) { CryptoProviderFactory = cryptoProviderFactory };
        var expiresAt = expires ?? DateTime.UtcNow.AddMinutes(10);
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Audience = audience,
            Subject = new ClaimsIdentity([new Claim(ClaimTypes.Name, "tests")]),
            NotBefore = expiresAt.AddHours(-1),
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256)
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    private static (RSAParameters Parameters, string PublicPem) CreateSigningKey()
    {
        using var rsa = RSA.Create(2048);
        return (rsa.ExportParameters(true), rsa.ExportRSAPublicKeyPem());
    }
}
