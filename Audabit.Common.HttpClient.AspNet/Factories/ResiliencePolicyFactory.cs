using Audabit.Common.HttpClient.AspNet.Settings;
using Audabit.Common.HttpClient.AspNet.Telemetry;
using Audabit.Common.Observability.Emitters;
using Audabit.Common.Observability.Extensions;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace Audabit.Common.HttpClient.AspNet.Factories;

/// <summary>
/// Factory for creating Polly resilience policies (retry, circuit breaker, and timeout).
/// </summary>
/// <remarks>
/// <para>
/// <b>Factory Overview:</b>
/// Provides static methods for creating configured Polly policies with fallback logic
/// between client-specific settings and default settings. This factory centralizes
/// policy creation to ensure consistent resilience patterns across all HTTP clients.
/// </para>
/// <para>
/// <b>Configuration Hierarchy:</b>
/// Each policy method accepts both client-specific settings and default settings,
/// using client settings when available and falling back to defaults otherwise.
/// This allows global defaults with per-client overrides.
/// </para>
/// <para>
/// <b>Policy Types:</b>
/// <list type="bullet">
/// <item><description>Timeout Policy - Cancels requests exceeding configured duration</description></item>
/// <item><description>Retry Policy - Retries failed requests with exponential backoff</description></item>
/// <item><description>Circuit Breaker Policy - Opens circuit when failure threshold exceeded</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Telemetry Integration:</b>
/// All policies emit telemetry events via <see cref="IEmitter{T}"/> for observability.
/// Events are raised for timeouts, retries, and circuit breaker state changes.
/// </para>
/// <para>
/// <b>Integration Notes:</b>
/// This factory is called by <see cref="Builders.ResilientHttpClientBuilder"/> during
/// HTTP client registration. Policies are applied in order: Timeout → Retry → Circuit Breaker.
/// </para>
/// </remarks>
/// <example>
/// Creating a timeout policy:
/// <code>
/// var timeoutPolicy = ResiliencePolicyFactory.CreateTimeoutPolicy&lt;WeatherApiClient&gt;(
///     emitter: emitter,
///     clientSettings: clientSettings,
///     defaultSettings: defaultSettings,
///     onTimeout: async (context, timeout, task) =&gt; 
///     {
///         await Console.Out.WriteLineAsync($"Request timed out after {timeout}");
///     });
/// </code>
/// 
/// Creating a retry policy with custom callback:
/// <code>
/// var retryPolicy = ResiliencePolicyFactory.CreateRetryPolicy&lt;WeatherApiClient&gt;(
///     emitter: emitter,
///     clientSettings: clientSettings,
///     defaultSettings: defaultSettings,
///     onRetry: (outcome, delay, attempt, context) =&gt; 
///     {
///         Console.WriteLine($"Retry {attempt} after {delay} - {outcome.Exception?.Message}");
///     });
/// </code>
/// 
/// Creating a circuit breaker policy:
/// <code>
/// var circuitBreakerPolicy = ResiliencePolicyFactory.CreateCircuitBreakerPolicy&lt;WeatherApiClient&gt;(
///     emitter: emitter,
///     clientSettings: clientSettings,
///     defaultSettings: defaultSettings,
///     onBreak: (outcome, duration, context) =&gt; 
///     {
///         Console.WriteLine($"Circuit opened for {duration}");
///     },
///     onReset: (context) =&gt; 
///     {
///         Console.WriteLine("Circuit reset");
///     });
/// </code>
/// </example>
public static class ResiliencePolicyFactory
{

    /// <summary>
    /// Creates a timeout policy.
    /// </summary>
    /// <typeparam name="TClient">The client type for logging.</typeparam>
    /// <param name="emitter">The emitter for logging events.</param>
    /// <param name="clientSettings">Client-specific settings.</param>
    /// <param name="defaultSettings">Default settings fallback.</param>
    /// <param name="onTimeout">Optional callback for timeout events.</param>
    /// <returns>A configured timeout policy.</returns>
    public static AsyncPolicy<HttpResponseMessage> CreateTimeoutPolicy<TClient>(
        IEmitter<TClient>? emitter,
        ClientSettings clientSettings,
        DefaultSettings defaultSettings,
        Func<Context, TimeSpan, Task, Task>? onTimeout = null)
    {
        ArgumentNullException.ThrowIfNull(clientSettings);
        ArgumentNullException.ThrowIfNull(defaultSettings);

        var timeoutSeconds = clientSettings.Timeout?.TimeoutSeconds ?? defaultSettings.Timeout.TimeoutSeconds;

        return Policy.TimeoutAsync<HttpResponseMessage>(
            timeout: TimeSpan.FromSeconds(timeoutSeconds),
            onTimeoutAsync: onTimeout ??
                (async (context, timespan, task) =>
                {
                    await Task.CompletedTask.ConfigureAwait(false);
                    emitter?.RaiseWarning(new HttpClientTimeoutEvent(
                        typeof(TClient).Name,
                        timespan.TotalSeconds));
                }));
    }

    /// <summary>
    /// Creates a retry policy with exponential backoff.
    /// </summary>
    /// <typeparam name="TClient">The client type for logging.</typeparam>
    /// <param name="emitter">The emitter for logging events.</param>
    /// <param name="clientSettings">Client-specific settings.</param>
    /// <param name="defaultSettings">Default settings fallback.</param>
    /// <param name="onRetry">Optional callback for retry events.</param>
    /// <returns>A configured retry policy.</returns>
    public static AsyncRetryPolicy<HttpResponseMessage> CreateRetryPolicy<TClient>(
        IEmitter<TClient>? emitter,
        ClientSettings clientSettings,
        DefaultSettings defaultSettings,
        Action<DelegateResult<HttpResponseMessage>, TimeSpan, int, Context>? onRetry = null)
    {
        ArgumentNullException.ThrowIfNull(clientSettings);
        ArgumentNullException.ThrowIfNull(defaultSettings);

        var maxRetryAttempts = clientSettings.Retry?.MaxRetryAttempts ?? defaultSettings.Retry.MaxRetryAttempts;
        var retryDelayMs = clientSettings.Retry?.RetryDelayMilliseconds ?? defaultSettings.Retry.RetryDelayMilliseconds;
        var backoffPower = clientSettings.Retry?.RetryBackoffPower ?? defaultSettings.Retry.RetryBackoffPower;
        var statusCodes = clientSettings.HttpRetryStatusCodes ?? defaultSettings.HttpRetryStatusCodes;

        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r => statusCodes.Contains((int)r.StatusCode))
            .WaitAndRetryAsync(
                retryCount: maxRetryAttempts,
                sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(retryDelayMs * Math.Pow(backoffPower, retryAttempt - 1)),
                onRetry: onRetry ??
                    ((outcome, timespan, retryCount, context) =>
                        emitter?.RaiseWarning(new HttpClientRetryEvent(
                            typeof(TClient).Name,
                            retryCount,
                            timespan.TotalMilliseconds,
                            outcome.Exception?.Message ?? $"Status code: {outcome.Result?.StatusCode}"))));
    }

    /// <summary>
    /// Creates an advanced circuit breaker policy.
    /// </summary>
    /// <typeparam name="TClient">The client type for logging.</typeparam>
    /// <param name="emitter">The emitter for logging events.</param>
    /// <param name="clientSettings">Client-specific settings.</param>
    /// <param name="defaultSettings">Default settings fallback.</param>
    /// <param name="onBreak">Optional callback for circuit breaker break events.</param>
    /// <param name="onReset">Optional callback for circuit breaker reset events.</param>
    /// <param name="onHalfOpen">Optional callback for circuit breaker half-open events.</param>
    /// <returns>A configured circuit breaker policy.</returns>
    public static AsyncCircuitBreakerPolicy<HttpResponseMessage> CreateCircuitBreakerPolicy<TClient>(
        IEmitter<TClient>? emitter,
        ClientSettings clientSettings,
        DefaultSettings defaultSettings,
        Action<DelegateResult<HttpResponseMessage>, TimeSpan, Context>? onBreak = null,
        Action<Context>? onReset = null,
        Action? onHalfOpen = null)
    {
        ArgumentNullException.ThrowIfNull(clientSettings);
        ArgumentNullException.ThrowIfNull(defaultSettings);

        var failureThreshold = clientSettings.CircuitBreaker?.FailureThreshold ?? defaultSettings.CircuitBreaker.FailureThreshold;
        var minimumThroughput = clientSettings.CircuitBreaker?.MinimumThroughput ?? defaultSettings.CircuitBreaker.MinimumThroughput;
        var sampleDurationMs = clientSettings.CircuitBreaker?.SampleDurationMilliseconds ?? defaultSettings.CircuitBreaker.SampleDurationMilliseconds;
        var durationOfBreakMs = clientSettings.CircuitBreaker?.DurationOfBreakMilliseconds ?? defaultSettings.CircuitBreaker.DurationOfBreakMilliseconds;
        var statusCodes = clientSettings.HttpRetryStatusCodes ?? defaultSettings.HttpRetryStatusCodes;

        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r => statusCodes.Contains((int)r.StatusCode))
            .AdvancedCircuitBreakerAsync(
                failureThreshold: failureThreshold,
                samplingDuration: TimeSpan.FromMilliseconds(sampleDurationMs),
                minimumThroughput: minimumThroughput,
                durationOfBreak: TimeSpan.FromMilliseconds(durationOfBreakMs),
                onBreak: onBreak ??
                    ((outcome, breakDuration, context) =>
                        emitter?.RaiseError(new HttpClientCircuitBreakerOpenedEvent(
                            typeof(TClient).Name,
                            breakDuration.TotalMilliseconds,
                            outcome.Exception?.Message ?? $"Status code: {outcome.Result?.StatusCode}"))),
                onReset: onReset ??
                    (context =>
                        emitter?.RaiseInformation(new HttpClientCircuitBreakerResetEvent(typeof(TClient).Name))),
                onHalfOpen: onHalfOpen ?? (() =>
                {
                    // Note: Can't access logger here without context
                }));
    }
}