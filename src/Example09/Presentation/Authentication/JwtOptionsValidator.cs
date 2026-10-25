using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace Example09.Presentation.Authentication;

public sealed class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
    public ValidateOptionsResult Validate(string name, JwtOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            failures.Add("Authentication:Issuer must be set.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            failures.Add("Authentication:Audience must be set.");
        }

        if (string.IsNullOrWhiteSpace(options.PublicKeyPem))
        {
            failures.Add("Authentication:PublicKeyPem must be set.");
        }
        else if (!CanImportPublicKey(options.PublicKeyPem))
        {
            failures.Add("Authentication:PublicKeyPem must be an RSA public key in PEM format.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private static bool CanImportPublicKey(string publicKeyPem)
    {
        using var rsa = RSA.Create();
        try
        {
            rsa.ImportFromPem(publicKeyPem);
            return true;
        }
        catch (Exception exception) when (exception is ArgumentException or CryptographicException)
        {
            return false;
        }
    }
}
