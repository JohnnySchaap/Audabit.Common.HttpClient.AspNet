using System.Globalization;
using Audabit.Common.HttpClient.AspNet.Extensions;
using Audabit.Common.HttpClient.AspNet.Handlers;
using Audabit.Common.HttpClient.AspNet.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Audabit.Common.HttpClient.AspNet.Tests.Unit.Extensions;

public class ResilientHttpClientExtensionsTests
{
    [Fact]
    public void AddResilientHttpClients_WithValidConfiguration_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new HttpClientSettings
        {
            DefaultSettings = new DefaultSettings(),
            Clients = []
        });

        // Act
        var builder = services.AddResilientHttpClients(configuration);

        // Assert
        builder.ShouldNotBeNull();
        var serviceProvider = services.BuildServiceProvider();
        serviceProvider.GetService<CorrelationIdDelegatingHandler>().ShouldNotBeNull();
    }

    [Fact]
    public void AddResilientHttpClients_WithNullServices_ShouldThrowArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;
        var configuration = CreateConfiguration(new HttpClientSettings());

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => services.AddResilientHttpClients(configuration));
    }

    [Fact]
    public void AddResilientHttpClients_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        IConfiguration configuration = null!;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => services.AddResilientHttpClients(configuration));
    }

    private static IConfiguration CreateConfiguration(HttpClientSettings settings)
    {
        var configData = new Dictionary<string, string?>();

        // Default settings
        if (settings.DefaultSettings != null)
        {
            configData["DefaultSettings:Timeout:TimeoutSeconds"] = settings.DefaultSettings.Timeout?.TimeoutSeconds.ToString();
            configData["DefaultSettings:Retry:MaxRetryAttempts"] = settings.DefaultSettings.Retry?.MaxRetryAttempts.ToString();
            configData["DefaultSettings:Retry:RetryDelayMilliseconds"] = settings.DefaultSettings.Retry?.RetryDelayMilliseconds.ToString();
            configData["DefaultSettings:Retry:RetryBackoffPower"] = settings.DefaultSettings.Retry?.RetryBackoffPower.ToString();
            configData["DefaultSettings:CircuitBreaker:FailureThreshold"] = settings.DefaultSettings.CircuitBreaker?.FailureThreshold.ToString(CultureInfo.InvariantCulture);
            configData["DefaultSettings:CircuitBreaker:MinimumThroughput"] = settings.DefaultSettings.CircuitBreaker?.MinimumThroughput.ToString();
            configData["DefaultSettings:CircuitBreaker:SampleDurationMilliseconds"] = settings.DefaultSettings.CircuitBreaker?.SampleDurationMilliseconds.ToString();
            configData["DefaultSettings:CircuitBreaker:DurationOfBreakMilliseconds"] = settings.DefaultSettings.CircuitBreaker?.DurationOfBreakMilliseconds.ToString();
            configData["DefaultSettings:Logging:LogRequest"] = settings.DefaultSettings.Logging?.LogRequest.ToString();
            configData["DefaultSettings:Logging:LogRequestHeaders"] = settings.DefaultSettings.Logging?.LogRequestHeaders.ToString();
            configData["DefaultSettings:Logging:LogResponse"] = settings.DefaultSettings.Logging?.LogResponse.ToString();
            configData["DefaultSettings:Logging:LogResponseHeaders"] = settings.DefaultSettings.Logging?.LogResponseHeaders.ToString();

            if (settings.DefaultSettings.HttpRetryStatusCodes != null)
            {
                for (var i = 0; i < settings.DefaultSettings.HttpRetryStatusCodes.Count; i++)
                {
                    configData[$"DefaultSettings:HttpRetryStatusCodes:{i}"] = settings.DefaultSettings.HttpRetryStatusCodes[i].ToString();
                }
            }
        }

        // Client settings
        if (settings.Clients != null)
        {
            foreach (var (key, value) in settings.Clients)
            {
                configData[$"Clients:{key}:BaseUrl"] = value.BaseUrl;

                if (value.Timeout != null)
                {
                    configData[$"Clients:{key}:Timeout:TimeoutSeconds"] = value.Timeout.TimeoutSeconds.ToString();
                }

                if (value.Retry != null)
                {
                    configData[$"Clients:{key}:Retry:MaxRetryAttempts"] = value.Retry.MaxRetryAttempts.ToString();
                    configData[$"Clients:{key}:Retry:RetryDelayMilliseconds"] = value.Retry.RetryDelayMilliseconds.ToString();
                    configData[$"Clients:{key}:Retry:RetryBackoffPower"] = value.Retry.RetryBackoffPower.ToString(CultureInfo.InvariantCulture);
                }

                if (value.CircuitBreaker != null)
                {
                    configData[$"Clients:{key}:CircuitBreaker:FailureThreshold"] = value.CircuitBreaker.FailureThreshold.ToString(CultureInfo.InvariantCulture);
                    configData[$"Clients:{key}:CircuitBreaker:MinimumThroughput"] = value.CircuitBreaker.MinimumThroughput.ToString();
                    configData[$"Clients:{key}:CircuitBreaker:SampleDurationMilliseconds"] = value.CircuitBreaker.SampleDurationMilliseconds.ToString();
                    configData[$"Clients:{key}:CircuitBreaker:DurationOfBreakMilliseconds"] = value.CircuitBreaker.DurationOfBreakMilliseconds.ToString();
                }

                if (value.Logging != null)
                {
                    configData[$"Clients:{key}:Logging:LogRequest"] = value.Logging.LogRequest.ToString();
                    configData[$"Clients:{key}:Logging:LogRequestHeaders"] = value.Logging.LogRequestHeaders.ToString();
                    configData[$"Clients:{key}:Logging:LogResponse"] = value.Logging.LogResponse.ToString();
                    configData[$"Clients:{key}:Logging:LogResponseHeaders"] = value.Logging.LogResponseHeaders.ToString();
                }

                if (value.HttpRetryStatusCodes != null)
                {
                    for (var i = 0; i < value.HttpRetryStatusCodes.Count; i++)
                    {
                        configData[$"Clients:{key}:HttpRetryStatusCodes:{i}"] = value.HttpRetryStatusCodes[i].ToString();
                    }
                }
            }
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
    }
}