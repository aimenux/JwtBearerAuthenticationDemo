using Microsoft.Extensions.Options;

namespace Example02.Presentation.Authentication;

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

        if (string.IsNullOrWhiteSpace(options.SigningKey))
        {
            failures.Add("Authentication:SigningKey must be set.");
        }
        else if (options.SigningKey.Length < JwtConstants.MinimumSigningKeyLength)
        {
            failures.Add($"Authentication:SigningKey must be at least {JwtConstants.MinimumSigningKeyLength} characters.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
