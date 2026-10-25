using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Example02.Tests.IntegrationTests.Fixtures;

internal class EmptySigningKeyWebApiTestsFixture : WebApplicationFactory<Program>
{
    private readonly string _signingKey;

    public EmptySigningKeyWebApiTestsFixture(string signingKey)
    {
        _signingKey = signingKey;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Authentication:SigningKey"] = _signingKey
            });
        });
    }
}
