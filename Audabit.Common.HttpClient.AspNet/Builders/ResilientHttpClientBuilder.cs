using Audabit.Common.HttpClient.AspNet.Factories;
using Audabit.Common.HttpClient.AspNet.Handlers;
using Audabit.Common.HttpClient.AspNet.Settings;
using Audabit.Common.Observability.Emitters;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace Audabit.Common.HttpClient.AspNet.Builders;

/// <summary>
/// Builder for configuring resilient HTTP clients with fault tolerance policies.
/// </summary>
/// <remarks>
/// <para>
/// <b>Builder Overview:</b>
/// This builder provides a fluent API for registering named HTTP clients with various combinations
/// of features including resilience policies (retry, circuit breaker, timeout), correlation ID
/// propagation, and structured logging. The builder pattern allows for flexible configuration while
/// maintaining clean separation of concerns.
/// </para>
/// <para>
/// <b>Available Feature Combinations:</b>
/// <list type="bullet">
/// <item><description><see cref="AddResilientHttpClient{TClient}"/> - All features (Polly + Correlation + Logging)</description></item>
/// <item><description><see cref="AddHttpClient{TClient}"/> - Basic client with no additional handlers</description></item>
/// <item><description><see cref="AddHttpClientWithResilience{TClient}"/> - Polly resilience only</description></item>
/// <item><description><see cref="AddHttpClientWithCorrelationId{TClient}"/> - Correlation ID only</description></item>
/// <item><description><see cref="AddHttpClientWithLogging{TClient}"/> - Logging only</description></item>
/// <item><description><see cref="AddHttpClientWithResilienceAndCorrelationId{TClient}"/> - Polly + Correlation</description></item>
/// <item><description><see cref="AddHttpClientWithResilienceAndLogging{TClient}"/> - Polly + Logging</description></item>
/// <item><description><see cref="AddHttpClientWithCorrelationIdAndLogging{TClient}"/> - Correlation + Logging</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Polly Resilience Features:</b>
/// When Polly is enabled, the following policies are applied in order:
/// <list type="number">
/// <item><description>Timeout Policy - Cancels requests that exceed configured timeout</description></item>
/// <item><description>Retry Policy - Retries failed requests with exponential backoff</description></item>
/// <item><description>Circuit Breaker - Prevents cascading failures by opening circuit after threshold</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Timeout Handling:</b>
/// When Polly resilience is enabled, HttpClient.Timeout is set to InfiniteTimeSpan and timeout
/// is managed by Polly's timeout policy. When Polly is not enabled, HttpClient.Timeout is set
/// to the configured value from settings.
/// </para>
/// <para>
/// <b>Configuration:</b>
/// All clients are configured via <see cref="HttpClientSettings"/> loaded from appsettings.json
/// or environment variables. Each client requires a named section under HttpClient:Clients:{ClientName}.
/// </para>
/// <para>
/// <b>Integration Notes:</b>
/// The builder internally uses <see cref="ResiliencePolicyFactory"/> to create Polly policies,
/// and registers handlers in the correct order: Polly (outermost), then Correlation ID, then Logging (innermost).
/// This ensures correlation IDs are propagated and logging captures the final request/response.
/// </para>
/// </remarks>
/// <example>
/// Register a fully-featured resilient HTTP client in Program.cs:
/// <code>
/// // 1. Configure in appsettings.json
/// {
///   "HttpClient": {
///     "Clients": {
///       "WeatherApiClient": {
///         "BaseUrl": "https://api.weather.com",
///         "TimeoutSeconds": 30,
///         "Retry": {
///           "MaxRetryAttempts": 3,
///           "RetryDelayMilliseconds": 100,
///           "RetryBackoffPower": 2
///         },
///         "CircuitBreaker": {
///           "FailureThreshold": 0.5,
///           "SamplingDurationSeconds": 10,
///           "MinimumThroughput": 5,
///           "DurationOfBreakSeconds": 30
///         },
///         "Logging": {
///           "LogRequest": true,
///           "LogResponse": true,
///           "LogRequestBody": false,
///           "LogResponseBody": true
///         }
///       }
///     }
///   }
/// }
/// 
/// // 2. Register in Program.cs
/// builder.Services
///     .AddHttpClientWithPolly(configuration.GetSection("HttpClient"))
///     .AddResilientHttpClient&lt;WeatherApiClient&gt;(
///         onTimeout: async (context, timeout, task) =&gt; 
///         {
///             // Custom timeout handling
///             await Console.Out.WriteLineAsync($"Request timed out after {timeout}");
///         },
///         onRetry: (outcome, delay, attempt, context) =&gt; 
///         {
///             // Custom retry logging
///             Console.WriteLine($"Retry attempt {attempt} after {delay}");
///         });
/// 
/// // 3. Inject and use the client
/// public class WeatherService(IHttpClientFactory factory)
/// {
///     public async Task&lt;WeatherData&gt; GetWeatherAsync()
///     {
///         var client = factory.CreateClient(nameof(WeatherApiClient));
///         var response = await client.GetAsync("/forecast");
///         return await response.Content.ReadFromJsonAsync&lt;WeatherData&gt;();
///     }
/// }
/// </code>
/// 
/// Register a basic client without resilience:
/// <code>
/// builder.Services
///     .AddHttpClientWithPolly(configuration.GetSection("HttpClient"))
///     .AddHttpClient&lt;SimpleApiClient&gt;();
/// </code>
/// 
/// Register a client with only logging:
/// <code>
/// builder.Services
///     .AddHttpClientWithPolly(configuration.GetSection("HttpClient"))
///     .AddHttpClientWithLogging&lt;LoggedApiClient&gt;();
/// </code>
/// </example>
/// <remarks>
/// Initializes a new instance of the <see cref="ResilientHttpClientBuilder"/> class.
/// </remarks>
/// <param name="services">The service collection.</param>
/// <param name="httpClientSettings">The HTTP client settings.</param>
public class ResilientHttpClientBuilder(IServiceCollection services, HttpClientSettings httpClientSettings)
{

    /// <summary>
    /// Adds an HTTP client with all features: Polly resilience policies, correlation ID, and logging.
    /// </summary>
    /// <typeparam name="TClient">The client type whose name will be used as the HTTP client name.</typeparam>
    /// <param name="onTimeout">Optional callback for timeout events.</param>
    /// <param name="onRetry">Optional callback for retry events.</param>
    /// <param name="onBreak">Optional callback for circuit breaker break events.</param>
    /// <param name="onReset">Optional callback for circuit breaker reset events.</param>
    /// <param name="onHalfOpen">Optional callback for circuit breaker half-open events.</param>
    /// <returns>The builder for chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no settings are found for the client.</exception>
    public ResilientHttpClientBuilder AddResilientHttpClient<TClient>(
        Func<Context, TimeSpan, Task, Task>? onTimeout = null,
        Action<DelegateResult<HttpResponseMessage>, TimeSpan, int, Context>? onRetry = null,
        Action<DelegateResult<HttpResponseMessage>, TimeSpan, Context>? onBreak = null,
        Action<Context>? onReset = null,
        Action? onHalfOpen = null)
    {
        return AddHttpClientCore<TClient>(includePolly: true, includeCorrelationId: true, includeLogging: true, onTimeout, onRetry, onBreak, onReset, onHalfOpen);
    }

    /// <summary>
    /// Adds a basic HTTP client without any handlers or policies.
    /// </summary>
    /// <typeparam name="TClient">The client type whose name will be used as the HTTP client name.</typeparam>
    /// <returns>The builder for chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no settings are found for the client.</exception>
    public ResilientHttpClientBuilder AddHttpClient<TClient>()
    {
        return AddHttpClientCore<TClient>(includePolly: false, includeCorrelationId: false, includeLogging: false);
    }

    /// <summary>
    /// Adds an HTTP client with only resilience policies (retry and circuit breaker).
    /// </summary>
    /// <typeparam name="TClient">The client type whose name will be used as the HTTP client name.</typeparam>
    /// <param name="onTimeout">Optional callback for timeout events.</param>
    /// <param name="onRetry">Optional callback for retry events.</param>
    /// <param name="onBreak">Optional callback for circuit breaker break events.</param>
    /// <param name="onReset">Optional callback for circuit breaker reset events.</param>
    /// <param name="onHalfOpen">Optional callback for circuit breaker half-open events.</param>
    /// <returns>The builder for chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no settings are found for the client.</exception>
    public ResilientHttpClientBuilder AddHttpClientWithResilience<TClient>(
        Func<Context, TimeSpan, Task, Task>? onTimeout = null,
        Action<DelegateResult<HttpResponseMessage>, TimeSpan, int, Context>? onRetry = null,
        Action<DelegateResult<HttpResponseMessage>, TimeSpan, Context>? onBreak = null,
        Action<Context>? onReset = null,
        Action? onHalfOpen = null)
    {
        return AddHttpClientCore<TClient>(includePolly: true, includeCorrelationId: false, includeLogging: false, onTimeout, onRetry, onBreak, onReset, onHalfOpen);
    }

    /// <summary>
    /// Adds an HTTP client with only correlation ID propagation.
    /// </summary>
    /// <typeparam name="TClient">The client type whose name will be used as the HTTP client name.</typeparam>
    /// <returns>The builder for chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no settings are found for the client.</exception>
    public ResilientHttpClientBuilder AddHttpClientWithCorrelationId<TClient>()
    {
        return AddHttpClientCore<TClient>(includePolly: false, includeCorrelationId: true, includeLogging: false);
    }

    /// <summary>
    /// Adds an HTTP client with only logging.
    /// </summary>
    /// <typeparam name="TClient">The client type whose name will be used as the HTTP client name.</typeparam>
    /// <returns>The builder for chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no settings are found for the client.</exception>
    public ResilientHttpClientBuilder AddHttpClientWithLogging<TClient>()
    {
        return AddHttpClientCore<TClient>(includePolly: false, includeCorrelationId: false, includeLogging: true);
    }

    /// <summary>
    /// Adds an HTTP client with resilience policies and correlation ID propagation.
    /// </summary>
    /// <typeparam name="TClient">The client type whose name will be used as the HTTP client name.</typeparam>
    /// <param name="onTimeout">Optional callback for timeout events.</param>
    /// <param name="onRetry">Optional callback for retry events.</param>
    /// <param name="onBreak">Optional callback for circuit breaker break events.</param>
    /// <param name="onReset">Optional callback for circuit breaker reset events.</param>
    /// <param name="onHalfOpen">Optional callback for circuit breaker half-open events.</param>
    /// <returns>The builder for chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no settings are found for the client.</exception>
    public ResilientHttpClientBuilder AddHttpClientWithResilienceAndCorrelationId<TClient>(
        Func<Context, TimeSpan, Task, Task>? onTimeout = null,
        Action<DelegateResult<HttpResponseMessage>, TimeSpan, int, Context>? onRetry = null,
        Action<DelegateResult<HttpResponseMessage>, TimeSpan, Context>? onBreak = null,
        Action<Context>? onReset = null,
        Action? onHalfOpen = null)
    {
        return AddHttpClientCore<TClient>(includePolly: true, includeCorrelationId: true, includeLogging: false, onTimeout, onRetry, onBreak, onReset, onHalfOpen);
    }

    /// <summary>
    /// Adds an HTTP client with resilience policies and logging.
    /// </summary>
    /// <typeparam name="TClient">The client type whose name will be used as the HTTP client name.</typeparam>
    /// <param name="onTimeout">Optional callback for timeout events.</param>
    /// <param name="onRetry">Optional callback for retry events.</param>
    /// <param name="onBreak">Optional callback for circuit breaker break events.</param>
    /// <param name="onReset">Optional callback for circuit breaker reset events.</param>
    /// <param name="onHalfOpen">Optional callback for circuit breaker half-open events.</param>
    /// <returns>The builder for chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no settings are found for the client.</exception>
    public ResilientHttpClientBuilder AddHttpClientWithResilienceAndLogging<TClient>(
        Func<Context, TimeSpan, Task, Task>? onTimeout = null,
        Action<DelegateResult<HttpResponseMessage>, TimeSpan, int, Context>? onRetry = null,
        Action<DelegateResult<HttpResponseMessage>, TimeSpan, Context>? onBreak = null,
        Action<Context>? onReset = null,
        Action? onHalfOpen = null)
    {
        return AddHttpClientCore<TClient>(includePolly: true, includeCorrelationId: false, includeLogging: true, onTimeout, onRetry, onBreak, onReset, onHalfOpen);
    }

    /// <summary>
    /// Adds an HTTP client with correlation ID propagation and logging.
    /// </summary>
    /// <typeparam name="TClient">The client type whose name will be used as the HTTP client name.</typeparam>
    /// <returns>The builder for chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no settings are found for the client.</exception>
    public ResilientHttpClientBuilder AddHttpClientWithCorrelationIdAndLogging<TClient>()
    {
        return AddHttpClientCore<TClient>(includePolly: false, includeCorrelationId: true, includeLogging: true);
    }

    private ResilientHttpClientBuilder AddHttpClientCore<TClient>(
        bool includePolly,
        bool includeCorrelationId,
        bool includeLogging,
        Func<Context, TimeSpan, Task, Task>? onTimeout = null,
        Action<DelegateResult<HttpResponseMessage>, TimeSpan, int, Context>? onRetry = null,
        Action<DelegateResult<HttpResponseMessage>, TimeSpan, Context>? onBreak = null,
        Action<Context>? onReset = null,
        Action? onHalfOpen = null)
    {
        var clientName = typeof(TClient).Name;
        var clientSettings = GetClientSettings(clientName);

        var httpClientBuilder = services.AddHttpClient(clientName, client => ConfigureHttpClient(client, clientSettings, includePolly));

        if (includeCorrelationId)
        {
            httpClientBuilder.AddHttpMessageHandler<CorrelationIdDelegatingHandler>();
        }

        if (includeLogging)
        {
            httpClientBuilder.AddHttpMessageHandler(sp => CreateLoggingHandler<TClient>(sp, clientSettings));
        }

        if (includePolly)
        {
            AddPollyPolicies<TClient>(httpClientBuilder, clientSettings, onTimeout, onRetry, onBreak, onReset, onHalfOpen);
        }

        return this;
    }

    private ClientSettings GetClientSettings(string clientName)
    {
        if (!httpClientSettings.Clients.TryGetValue(clientName, out var clientSettings))
        {
            throw new InvalidOperationException($"No HTTP client settings found for '{clientName}'");
        }
        return clientSettings;
    }

    private void ConfigureHttpClient(System.Net.Http.HttpClient client, ClientSettings clientSettings, bool includePolly)
    {
        client.BaseAddress = new Uri(clientSettings.BaseUrl);

        if (includePolly)
        {
            // Timeout is handled by Polly timeout policy in AddPollyPolicies
            client.Timeout = Timeout.InfiniteTimeSpan;
        }
        else
        {
            // Use configured timeout from settings
            var timeoutSeconds = clientSettings.Timeout?.TimeoutSeconds ?? httpClientSettings.DefaultSettings.Timeout.TimeoutSeconds;
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        }
    }

    private HttpLoggingDelegatingHandler<TClient> CreateLoggingHandler<TClient>(IServiceProvider sp, ClientSettings clientSettings)
    {
        var loggingSettings = clientSettings.Logging ?? httpClientSettings.DefaultSettings.Logging;
        return new HttpLoggingDelegatingHandler<TClient>(
            getEmitter: () => sp.GetService<IEmitter<TClient>>(),
            logRequest: loggingSettings.LogRequest,
            logRequestHeaders: loggingSettings.LogRequestHeaders,
            logRequestBody: loggingSettings.LogRequestBody,
            logResponse: loggingSettings.LogResponse,
            logResponseHeaders: loggingSettings.LogResponseHeaders,
            logResponseBody: loggingSettings.LogResponseBody);
    }

    private void AddPollyPolicies<TClient>(
        IHttpClientBuilder httpClientBuilder,
        ClientSettings clientSettings,
        Func<Context, TimeSpan, Task, Task>? onTimeout,
        Action<DelegateResult<HttpResponseMessage>, TimeSpan, int, Context>? onRetry,
        Action<DelegateResult<HttpResponseMessage>, TimeSpan, Context>? onBreak,
        Action<Context>? onReset,
        Action? onHalfOpen)
    {
        AsyncRetryPolicy<HttpResponseMessage>? retryPolicy = null;
        AsyncCircuitBreakerPolicy<HttpResponseMessage>? circuitBreakerPolicy = null;
        AsyncPolicy<HttpResponseMessage>? timeoutPolicy = null;

        httpClientBuilder
            .AddPolicyHandler((sp, _) =>
            {
                timeoutPolicy ??= ResiliencePolicyFactory.CreateTimeoutPolicy(sp.GetService<IEmitter<TClient>>(), clientSettings, httpClientSettings.DefaultSettings, onTimeout);
                return timeoutPolicy;
            })
            .AddPolicyHandler((sp, _) =>
            {
                retryPolicy ??= ResiliencePolicyFactory.CreateRetryPolicy(sp.GetService<IEmitter<TClient>>(), clientSettings, httpClientSettings.DefaultSettings, onRetry);
                return retryPolicy;
            })
            .AddPolicyHandler((sp, _) =>
            {
                circuitBreakerPolicy ??= ResiliencePolicyFactory.CreateCircuitBreakerPolicy(sp.GetService<IEmitter<TClient>>(), clientSettings, httpClientSettings.DefaultSettings, onBreak, onReset, onHalfOpen);
                return circuitBreakerPolicy;
            });
    }
}