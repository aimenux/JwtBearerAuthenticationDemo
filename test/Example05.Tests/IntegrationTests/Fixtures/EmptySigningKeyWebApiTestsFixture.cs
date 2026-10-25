using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Example05.Tests.IntegrationTests.Fixtures;

internal class EmptySigningKeyWebApiTestsFixture : WebApplicationFactory<Program>
{
    private readonly string _signingKey;

    public EmptySigningKeyWebApiTestsFixture(string signingKey)
    {
        _signingKey = signingKey;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Authentication:SigningKey"] = _signingKey
            });
        });
    }
}
