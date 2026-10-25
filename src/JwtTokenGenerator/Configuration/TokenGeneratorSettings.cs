namespace JwtTokenGenerator.Configuration;

public sealed record TokenGeneratorSettings(string DefaultSubject, int DefaultLifetimeMinutes, string RsaPrivateKeyPem);
