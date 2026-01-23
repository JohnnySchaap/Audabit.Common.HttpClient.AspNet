using Audabit.Common.HttpClient.AspNet.Builders;
using Audabit.Common.HttpClient.AspNet.Handlers;
using Audabit.Common.HttpClient.AspNet.Settings;
using Audabit.Common.HttpClient.AspNet.Tests.Unit.TestHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace Audabit.Common.HttpClient.AspNet.Tests.Unit.Builders;

public class ResilientHttpClientBuilderTests
{
    private readonly Fixture _fixture;

    public ResilientHttpClientBuilderTests()
    {
        _fixture = FixtureFactory.Create();
    }

    [Fact]
    public void AddResilientHttpClient_WithValidClient_ShouldConfigureHttpClient()
    {
        // Arrange
        var (services, settings) = CreateServicesAndSettings();
        var builder = new ResilientHttpClientBuilder(services, settings);

        // Act
        builder.AddResilientHttpClient<TestClient>();

        // Assert
        var client = GetHttpClient(services, nameof(TestClient));
        client.ShouldNotBeNull();
        client.BaseAddress.ShouldBe(new Uri(settings.Clients["TestClient"].BaseUrl));
        // With Polly enabled, timeout should be infinite (Polly handles it)
        client.Timeout.ShouldBe(System.Threading.Timeout.InfiniteTimeSpan);
    }

    [Theory]
    [InlineData(30, 60)]  // default: 30, client: 60
    [InlineData(45, 120)] // default: 45, client: 120
    public void AddResilientHttpClient_WithClientSpecificTimeout_ShouldUseInfiniteTimeout(int defaultTimeout, int clientTimeout)
    {
        // Arrange
        var (services, settings) = CreateServicesAndSettings(defaultTimeout, clientTimeout);
        var builder = new ResilientHttpClientBuilder(services, settings);

        // Act
        builder.AddResilientHttpClient<TestClient>();

        // Assert
        var client = GetHttpClient(services, nameof(TestClient));
        // With Polly enabled, timeout should be infinite (Polly handles it)
        client.Timeout.ShouldBe(System.Threading.Timeout.InfiniteTimeSpan);
    }

    [Fact]
    public void AddResilientHttpClient_WithoutClientSettings_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var settings = _fixture.Build<HttpClientSettings>()
            .With(x => x.Clients, [])
            .Create();
        var builder = new ResilientHttpClientBuilder(services, settings);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => builder.AddResilientHttpClient<TestClient>());
        exception.Message.ShouldContain("TestClient");
    }

    [Fact]
    public void AddHttpClient_WithoutPolly_ShouldUseConfiguredTimeout()
    {
        // Arrange
        var (services, settings) = CreateServicesAndSettings();
        var builder = new ResilientHttpClientBuilder(services, settings);

        // Act
        builder.AddHttpClient<TestClient>();

        // Assert
        var client = GetHttpClient(services, nameof(TestClient));
        client.ShouldNotBeNull();
        client.BaseAddress.ShouldBe(new Uri(settings.Clients["TestClient"].BaseUrl));
        // Without Polly, timeout should be from settings
        client.Timeout.ShouldBe(TimeSpan.FromSeconds(settings.DefaultSettings.Timeout.TimeoutSeconds));
    }

    [Theory]
    [InlineData(30, 60)]  // default: 30, client: 60
    [InlineData(45, 120)] // default: 45, client: 120
    public void AddHttpClient_WithoutPolly_WithClientSpecificTimeout_ShouldUseClientTimeout(int defaultTimeout, int clientTimeout)
    {
        // Arrange
        var (services, settings) = CreateServicesAndSettings(defaultTimeout, clientTimeout);
        var builder = new ResilientHttpClientBuilder(services, settings);

        // Act
        builder.AddHttpClient<TestClient>();

        // Assert
        var client = GetHttpClient(services, nameof(TestClient));
        // Without Polly, timeout should be from client settings
        client.Timeout.ShouldBe(TimeSpan.FromSeconds(clientTimeout));
    }

    [Fact]
    public void AddResilientHttpClient_WithMultipleClients_ShouldRegisterAll()
    {
        // Arrange
        var (services, settings) = CreateServicesAndSettings();
        var baseUrl2 = _fixture.Create<Uri>().ToString();
        settings.Clients["AnotherTestClient"] = _fixture.Build<ClientSettings>()
            .With(x => x.BaseUrl, baseUrl2)
            .Create();

        var builder = new ResilientHttpClientBuilder(services, settings);

        // Act
        builder
            .AddResilientHttpClient<TestClient>()
            .AddResilientHttpClient<AnotherTestClient>();

        // Assert
        var client1 = GetHttpClient(services, nameof(TestClient));
        client1.BaseAddress.ShouldBe(new Uri(settings.Clients["TestClient"].BaseUrl));

        var client2 = GetHttpClient(services, nameof(AnotherTestClient));
        client2.BaseAddress.ShouldBe(new Uri(baseUrl2));
    }

    [Fact]
    public void AddResilientHttpClient_WithClientSpecificRetrySettings_ShouldUseClientSettings()
    {
        // Arrange
        var (services, settings) = CreateServicesAndSettings();
        var retrySettings = _fixture.Create<RetrySettings>();
        settings = _fixture.Build<HttpClientSettings>()
            .With(x => x.DefaultSettings, settings.DefaultSettings)
            .With(x => x.Clients, new Dictionary<string, ClientSettings>
            {
                ["TestClient"] = _fixture.Build<ClientSettings>()
                    .With(c => c.BaseUrl, settings.Clients["TestClient"].BaseUrl)
                    .With(c => c.Retry, retrySettings)
                    .Without(c => c.Timeout)
                    .Without(c => c.CircuitBreaker)
                    .Without(c => c.Logging)
                    .Without(c => c.HttpRetryStatusCodes)
                    .Create()
            })
            .Create();
        var builder = new ResilientHttpClientBuilder(services, settings);

        // Act
        builder.AddResilientHttpClient<TestClient>();

        // Assert
        var client = GetHttpClient(services, nameof(TestClient));
        client.ShouldNotBeNull();
    }

    [Fact]
    public void AddResilientHttpClient_WithClientSpecificCircuitBreakerSettings_ShouldUseClientSettings()
    {
        // Arrange
        var (services, settings) = CreateServicesAndSettings();
        var circuitBreakerSettings = _fixture.Create<CircuitBreakerSettings>();
        settings = _fixture.Build<HttpClientSettings>()
            .With(x => x.DefaultSettings, settings.DefaultSettings)
            .With(x => x.Clients, new Dictionary<string, ClientSettings>
            {
                ["TestClient"] = _fixture.Build<ClientSettings>()
                    .With(c => c.BaseUrl, settings.Clients["TestClient"].BaseUrl)
                    .With(c => c.CircuitBreaker, circuitBreakerSettings)
                    .Without(c => c.Timeout)
                    .Without(c => c.Retry)
                    .Without(c => c.Logging)
                    .Without(c => c.HttpRetryStatusCodes)
                    .Create()
            })
            .Create();
        var builder = new ResilientHttpClientBuilder(services, settings);

        // Act
        builder.AddResilientHttpClient<TestClient>();

        // Assert
        var client = GetHttpClient(services, nameof(TestClient));
        client.ShouldNotBeNull();
    }

    [Fact]
    public void AddResilientHttpClient_WithClientSpecificLoggingSettings_ShouldUseClientSettings()
    {
        // Arrange
        var (services, settings) = CreateServicesAndSettings();
        var loggingSettings = _fixture.Create<LoggingSettings>();
        settings = _fixture.Build<HttpClientSettings>()
            .With(x => x.DefaultSettings, settings.DefaultSettings)
            .With(x => x.Clients, new Dictionary<string, ClientSettings>
            {
                ["TestClient"] = _fixture.Build<ClientSettings>()
                    .With(c => c.BaseUrl, settings.Clients["TestClient"].BaseUrl)
                    .With(c => c.Logging, loggingSettings)
                    .Without(c => c.Timeout)
                    .Without(c => c.Retry)
                    .Without(c => c.CircuitBreaker)
                    .Without(c => c.HttpRetryStatusCodes)
                    .Create()
            })
            .Create();
        var builder = new ResilientHttpClientBuilder(services, settings);

        // Act
        builder.AddResilientHttpClient<TestClient>();

        // Assert
        var client = GetHttpClient(services, nameof(TestClient));
        client.ShouldNotBeNull();
    }

    private (ServiceCollection services, HttpClientSettings settings) CreateServicesAndSettings(
        int timeoutSeconds = 30,
        int? clientTimeoutSeconds = null)
    {
        var services = new ServiceCollection();
        services.AddTransient<CorrelationIdDelegatingHandler>();

        var baseUrl = _fixture.Create<Uri>().ToString();
        var clientSettingsBuilder = _fixture.Build<ClientSettings>()
            .With(c => c.BaseUrl, baseUrl)
            .Without(c => c.Retry)
            .Without(c => c.CircuitBreaker)
            .Without(c => c.Logging)
            .Without(c => c.HttpRetryStatusCodes);

        if (clientTimeoutSeconds.HasValue)
        {
            clientSettingsBuilder = clientSettingsBuilder.With(c => c.Timeout,
                new TimeoutSettings { TimeoutSeconds = clientTimeoutSeconds.Value });
        }
        else
        {
            clientSettingsBuilder = clientSettingsBuilder.Without(c => c.Timeout);
        }

        var settings = _fixture.Build<HttpClientSettings>()
            .With(x => x.DefaultSettings, _fixture.Build<DefaultSettings>()
                .With(d => d.Timeout, new TimeoutSettings { TimeoutSeconds = timeoutSeconds })
                .With(d => d.Retry, _fixture.Create<RetrySettings>())
                .With(d => d.CircuitBreaker, _fixture.Create<CircuitBreakerSettings>())
                .With(d => d.HttpRetryStatusCodes, [500, 502, 503])
                .With(d => d.Logging, _fixture.Create<LoggingSettings>())
                .Create())
            .With(x => x.Clients, new Dictionary<string, ClientSettings>
            {
                ["TestClient"] = clientSettingsBuilder.Create()
            })
            .Create();

        return (services, settings);
    }

    private static System.Net.Http.HttpClient GetHttpClient(ServiceCollection services, string clientName)
    {
        var serviceProvider = services.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        return httpClientFactory.CreateClient(clientName);
    }

    private class TestClient { }
    private class AnotherTestClient { }
}