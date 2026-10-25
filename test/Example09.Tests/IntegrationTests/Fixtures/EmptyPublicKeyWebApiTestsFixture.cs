using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Example09.Tests.IntegrationTests.Fixtures;

internal class EmptyPublicKeyWebApiTestsFixture : WebApplicationFactory<Program>
{
    private readonly string _publicKeyPem;

    public EmptyPublicKeyWebApiTestsFixture(string publicKeyPem)
    {
        _publicKeyPem = publicKeyPem;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Authentication:PublicKeyPem"] = _publicKeyPem
            });
        });
    }
}
