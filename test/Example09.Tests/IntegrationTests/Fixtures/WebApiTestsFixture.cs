using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Example09.Presentation.Authentication;

namespace Example09.Tests.IntegrationTests.Fixtures;

internal class WebApiTestsFixture : WebApplicationFactory<Program>
{
    private readonly string _authorizationHeaderValue;

    public WebApiTestsFixture(string authorizationHeaderValue = null)
    {
        _authorizationHeaderValue = authorizationHeaderValue ?? $"Bearer {TestTokens.Create()}";
        ClientOptions.BaseAddress = new Uri("https://localhost:5000");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Authentication:Issuer"] = TestTokens.Issuer,
                ["Authentication:Audience"] = TestTokens.Audience,
                ["Authentication:PublicKeyPem"] = TestTokens.PublicKeyPem,
                ["Https:Port"] = "5000"
            });
        });
    }

    protected override void ConfigureClient(HttpClient client)
    {
        base.ConfigureClient(client);
        client
            .DefaultRequestHeaders
            .Add(JwtConstants.AuthorizationHeaderName, _authorizationHeaderValue);
    }
}
