using Audabit.Common.HttpClient.AspNet.Builders;
using Audabit.Common.HttpClient.AspNet.Handlers;
using Audabit.Common.HttpClient.AspNet.Settings;
using Audabit.Common.Validation.AspNet.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Audabit.Common.HttpClient.AspNet.Extensions;

/// <summary>
/// Extension methods for configuring HTTP clients with Polly resilience policies.
/// </summary>
public static class ResilientHttpClientExtensions
{
    /// <summary>
    /// Adds resilient HTTP clients with retry and circuit breaker policies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration section for HttpClientSettings.</param>
    /// <returns>A builder for configuring HTTP clients.</returns>
    public static ResilientHttpClientBuilder AddResilientHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<HttpClientSettings>()
            .Bind(configuration)
            .ValidateWithFluentValidation();

        // Register HTTP message handlers
        services.AddTransient<CorrelationIdDelegatingHandler>();

        var httpClientSettings = configuration.Get<HttpClientSettings>() ?? new HttpClientSettings();

        return new ResilientHttpClientBuilder(services, httpClientSettings);
    }
}