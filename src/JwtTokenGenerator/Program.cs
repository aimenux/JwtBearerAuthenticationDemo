using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using JwtTokenGenerator.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var jwtSettings = configuration.GetRequiredSection("Authentication").Get<JwtSettings>()!;
var generatorSettings = configuration.GetRequiredSection("TokenGenerator").Get<TokenGeneratorSettings>()!;

var subject = args.Length > 0 ? args[0] : generatorSettings.DefaultSubject;
var lifetimeMinutes = generatorSettings.DefaultLifetimeMinutes;
var algorithm = args.Length > 2 ? args[2] : SecurityAlgorithms.HmacSha256;

if (args.Length > 1 && !int.TryParse(args[1], out lifetimeMinutes))
{
    return Usage();
}

if (lifetimeMinutes <= 0)
{
    return Usage();
}

using var rsaPrivateKey = algorithm == SecurityAlgorithms.RsaSha256 ? RSA.Create() : null;
rsaPrivateKey?.ImportFromPem(generatorSettings.RsaPrivateKeyPem);

var signingCredentials = algorithm switch
{
    SecurityAlgorithms.HmacSha256 => new SigningCredentials(
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
        SecurityAlgorithms.HmacSha256),
    SecurityAlgorithms.RsaSha256 => new SigningCredentials(
        new RsaSecurityKey(rsaPrivateKey!),
        SecurityAlgorithms.RsaSha256),
    _ => null
};

if (signingCredentials is null)
{
    return Usage();
}

var claims = new List<Claim>
{
    new(JwtRegisteredClaimNames.Sub, subject),
    new(JwtRegisteredClaimNames.Name, subject)
};

var issuedAt = DateTime.UtcNow;
var descriptor = new SecurityTokenDescriptor
{
    Issuer = jwtSettings.Issuer,
    Audience = jwtSettings.Audience,
    Subject = new ClaimsIdentity(claims),
    NotBefore = issuedAt,
    Expires = issuedAt.AddMinutes(lifetimeMinutes),
    SigningCredentials = signingCredentials
};

Console.WriteLine(new JsonWebTokenHandler().CreateToken(descriptor));
return 0;

static int Usage()
{
    Console.Error.WriteLine("Usage: JwtTokenGenerator [subject] [lifetimeMinutes] [HS256|RS256]");
    return 1;
}
