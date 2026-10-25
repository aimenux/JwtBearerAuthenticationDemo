using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using AwesomeAssertions;
using Microsoft.AspNetCore.Builder;

namespace Example03.Tests.IntegrationTests;

public class WebApiTransportTests
{
    [Fact]
    public async Task Should_Serve_Https_On_Configured_Port()
    {
        // Arrange
        var port = GetFreePort();
        await using var app = CreateApp(port);
        await app.StartAsync();
        using var client = CreateHttpsClient();

        // Act
        var response = await client.GetAsync($"https://localhost:{port}/ping");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Should_Refuse_Plain_Http_On_Https_Port()
    {
        // Arrange
        var port = GetFreePort();
        await using var app = CreateApp(port);
        await app.StartAsync();
        using var client = CreateHttpsClient();

        // Act
        var act = () => client.GetAsync($"http://localhost:{port}/ping");

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public void Should_Refuse_To_Start_When_Https_Port_Is_Not_Configured()
    {
        // Act
        var act = () => WebApplication.CreateBuilder([], enforceHttps: true);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    private static WebApplication CreateApp(int port)
    {
        var builder = WebApplication.CreateBuilder(["--Https:Port=" + port], enforceHttps: true);
        builder.Logging.ClearProviders();

        var app = builder.Build();
        app.MapGet("/ping", () => "pong");
        return app;
    }

    private static HttpClient CreateHttpsClient() =>
        new(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        });

    private static int GetFreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }
}
