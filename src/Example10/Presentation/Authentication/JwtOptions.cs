namespace Example10.Presentation.Authentication;

public sealed record JwtOptions
{
    public const string SectionName = "Authentication";

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public string PublicKeyPem { get; init; } = string.Empty;
}
