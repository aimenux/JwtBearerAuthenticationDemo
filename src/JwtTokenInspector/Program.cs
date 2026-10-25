using System.Security.Cryptography;
using System.Text;
using JwtTokenInspector.Configuration;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: JwtTokenInspector <token>");
    return 1;
}

var token = args[0];

JsonWebToken jsonWebToken;
try
{
    jsonWebToken = new JsonWebToken(token);
}
catch (ArgumentException exception)
{
    Console.Error.WriteLine($"Not a readable JWT: {exception.Message}");
    return 1;
}

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();
var jwtSettings = configuration.GetRequiredSection("Authentication").Get<JwtSettings>()!;

Console.WriteLine("Header:");
Console.WriteLine(PrettyPrintJson(jsonWebToken.EncodedHeader));
Console.WriteLine("Payload:");
Console.WriteLine(PrettyPrintJson(jsonWebToken.EncodedPayload));
Console.WriteLine($"Subject:    {jsonWebToken.Subject}");
Console.WriteLine($"Issuer:     {jsonWebToken.Issuer}");
Console.WriteLine($"Audience:   {string.Join(", ", jsonWebToken.Audiences)}");
Console.WriteLine($"Not before: {jsonWebToken.ValidFrom:O}");
Console.WriteLine($"Expires:    {jsonWebToken.ValidTo:O}");

using var rsaPublicKey = RSA.Create();
if (!string.IsNullOrWhiteSpace(jwtSettings.PublicKeyPem))
{
    rsaPublicKey.ImportFromPem(jwtSettings.PublicKeyPem);
}

var families = new List<(string Name, string Algorithm, SecurityKey Key)>
{
    ("HS256 examples", SecurityAlgorithms.HmacSha256, new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)))
};
if (!string.IsNullOrWhiteSpace(jwtSettings.PublicKeyPem))
{
    families.Add(("RS256 examples (09-10)", SecurityAlgorithms.RsaSha256, new RsaSecurityKey(rsaPublicKey)));
}

var exitCode = 2;
var validated = false;
foreach (var (name, algorithm, key) in families)
{
    if (algorithm != jsonWebToken.Alg)
    {
        Console.WriteLine($"Validation ({name}): rejected, token alg is {jsonWebToken.Alg}, examples accept {algorithm}");
        continue;
    }

    validated = true;
    var validation = await new JsonWebTokenHandler().ValidateTokenAsync(
        token,
        CreateValidationParameters(jwtSettings, key, algorithm));
    if (validation.IsValid)
    {
        Console.WriteLine($"Validation ({name}): valid");
        exitCode = 0;
    }
    else
    {
        Console.WriteLine($"Validation ({name}): invalid ({validation.Exception?.Message})");
    }
}

if (!validated)
{
    Console.WriteLine($"Validation: invalid (no example accepts alg {jsonWebToken.Alg}; PublicKeyPem is not configured)");
}

return exitCode;

static string PrettyPrintJson(string base64UrlSegment)
{
    var json = Encoding.UTF8.GetString(Base64UrlEncoder.DecodeBytes(base64UrlSegment));
    using var document = JsonDocument.Parse(json);
    return JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions { WriteIndented = true });
}

static TokenValidationParameters CreateValidationParameters(JwtSettings settings, SecurityKey signingKey, string algorithm) => new()
{
    ValidIssuer = settings.Issuer,
    ValidAudience = settings.Audience,
    IssuerSigningKey = signingKey,
    ValidAlgorithms = [algorithm],
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateIssuerSigningKey = true,
    ValidateLifetime = true
};
