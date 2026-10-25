using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Example10.Presentation.Authentication;

public sealed class JwtTokenValidator : IDisposable
{
    private readonly JsonWebTokenHandler _tokenHandler = new();
    private readonly RSA _publicKey;
    private readonly TokenValidationParameters _validationParameters;

    public JwtTokenValidator(IOptions<JwtOptions> options)
    {
        var jwt = options.Value;
        _publicKey = RSA.Create();
        _publicKey.ImportFromPem(jwt.PublicKeyPem);
        _validationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new RsaSecurityKey(_publicKey),
            ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
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

    public void Dispose() => _publicKey.Dispose();
}
