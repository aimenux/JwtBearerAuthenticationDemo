namespace Example01;

public static class WebApplicationExtensions
{
    extension(WebApplication)
    {
        public static WebApplicationBuilder CreateBuilder(string[] args, bool enforceHttps)
        {
            var builder = WebApplication.CreateBuilder(args);

            if (enforceHttps)
            {
                var httpsPort = GetHttpsPort(builder.Configuration);

                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.ListenLocalhost(httpsPort, listen => listen.UseHttps());
                });

            }

            return builder;
        }
    }

    private static int GetHttpsPort(ConfigurationManager configuration)
    {
        var configuredPort = configuration.GetValue<int?>("Https:Port");
        if (configuredPort is { } port)
        {
            return port;
        }

        var httpsUri = (configuration["urls"] ?? string.Empty)
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(url => new Uri(url))
            .FirstOrDefault(uri => uri.Scheme == Uri.UriSchemeHttps);

        return httpsUri?.Port ?? throw new InvalidOperationException("No HTTPS port configured.");
    }
}
