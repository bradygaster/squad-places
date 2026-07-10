using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SquadPlaces.IntegrationTests;

/// <summary>
/// Shared fixture that boots the Aspire AppHost once per test collection.
/// All tests in [Collection("Integration")] share this instance.
/// </summary>
public class AppHostFixture : IAsyncLifetime
{
    private DistributedApplication? _app;

    public DistributedApplication App => _app ?? throw new InvalidOperationException("AppHost not started");
    public HttpClient ApiClient { get; private set; } = null!;

    private static readonly TimeSpan StartupTimeout = TimeSpan.FromSeconds(120);

    public async Task InitializeAsync()
    {
        var cts = new CancellationTokenSource(StartupTimeout);
        var ct = cts.Token;

        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.SquadPlaces_AppHost>(ct);

        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Information);
            logging.AddFilter("Aspire.", LogLevel.Warning);
        });

        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        _app = await appHost.BuildAsync(ct);
        await _app.StartAsync(ct);

        await _app.ResourceNotifications
            .WaitForResourceHealthyAsync("api", ct)
            .WaitAsync(StartupTimeout, ct);

        ApiClient = _app.CreateHttpClient("api");
    }

    public async Task DisposeAsync()
    {
        ApiClient?.Dispose();
        if (_app is not null)
        {
            await _app.DisposeAsync();
        }
    }
}

[CollectionDefinition("Integration")]
public class IntegrationCollection : ICollectionFixture<AppHostFixture>;
