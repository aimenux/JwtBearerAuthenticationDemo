namespace JwtTokenGenerator.Configuration;

public sealed record JwtSettings(string Issuer, string Audience, string SigningKey);
