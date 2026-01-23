namespace Audabit.Common.HttpClient.AspNet.Settings;

/// <summary>
/// Default settings applied to all HTTP clients unless overridden by client-specific configurations.
/// </summary>
/// <remarks>
/// <para>
/// These settings serve as the baseline configuration for all registered HTTP clients.
/// Individual clients can override any of these settings through <see cref="ClientSettings"/>.
/// </para>
/// <para>
/// All settings can be configured via appsettings.json or environment variables.
/// Environment variables use double underscore (__) as the hierarchy separator.
/// </para>
/// </remarks>
/// <example>
/// Configuration in appsettings.json:
/// <code>
/// {
///   "HttpClientSettings": {
///     "DefaultSettings": {
///       "Timeout": { "TimeoutSeconds": 30 },
///       "Retry": {
///         "MaxRetryAttempts": 3,
///         "RetryDelayMilliseconds": 500,
///         "RetryBackoffPower": 2
///       },
///       "CircuitBreaker": {
///         "FailureThreshold": 0.5,
///         "MinimumThroughput": 10,
///         "SampleDurationMilliseconds": 60000,
///         "DurationOfBreakMilliseconds": 10000
///       },
///       "HttpRetryStatusCodes": [500, 502, 503, 504, 408, 429],
///       "Logging": {
///         "LogRequestHeaders": true,
///         "LogResponseHeaders": true,
///         "LogRequestBody": false,
///         "LogResponseBody": false
///       }
///     }
///   }
/// }
/// </code>
/// 
/// Or using environment variables:
/// <code>
/// HttpClientSettings__DefaultSettings__Timeout__TimeoutSeconds=30
/// HttpClientSettings__DefaultSettings__Retry__MaxRetryAttempts=3
/// HttpClientSettings__DefaultSettings__CircuitBreaker__FailureThreshold=0.5
/// </code>
/// </example>
public sealed record DefaultSettings
{
    /// <summary>
    /// Gets or initializes the default timeout settings for all HTTP clients.
    /// </summary>
    /// <value>
    /// Timeout configuration with a default of 30 seconds.
    /// Never null - initialized with sensible defaults.
    /// </value>
    /// <remarks>
    /// The timeout applies per request attempt. With Polly policies enabled, individual retries
    /// each have this timeout. Can be overridden per client via <see cref="ClientSettings.Timeout"/>.
    /// </remarks>
    public TimeoutSettings Timeout { get; init; } = new();

    /// <summary>
    /// Gets or initializes the default retry settings for all HTTP clients.
    /// </summary>
    /// <value>
    /// Retry policy configuration with defaults: 3 attempts, 500ms initial delay, exponential backoff.
    /// Never null - initialized with sensible defaults.
    /// </value>
    /// <remarks>
    /// Retry is triggered by HTTP status codes in <see cref="HttpRetryStatusCodes"/> and network exceptions.
    /// Can be overridden per client via <see cref="ClientSettings.Retry"/>.
    /// </remarks>
    public RetrySettings Retry { get; init; } = new();

    /// <summary>
    /// Gets or initializes the default circuit breaker settings for all HTTP clients.
    /// </summary>
    /// <value>
    /// Circuit breaker policy configuration. Default threshold of 1.0 (100%) effectively disables
    /// circuit breaking unless configured. Never null - initialized with defaults.
    /// </value>
    /// <remarks>
    /// Circuit breaker monitors failure rates over a sample duration and "breaks" (stops requests)
    /// when the failure threshold is exceeded. Lower <see cref="CircuitBreakerSettings.FailureThreshold"/>
    /// to enable (e.g., 0.5 for 50% failure rate). Can be overridden per client.
    /// </remarks>
    public CircuitBreakerSettings CircuitBreaker { get; init; } = new();

    /// <summary>
    /// Gets or initializes the HTTP status codes that trigger retry and circuit breaker policies.
    /// </summary>
    /// <value>
    /// A list of HTTP status codes (500, 502, 503, 504, 408, 429) representing server errors
    /// and transient failures. Never null - initialized with common transient failure codes.
    /// </value>
    /// <remarks>
    /// <para>
    /// Default codes:
    /// <list type="bullet">
    /// <item><description>500 - InternalServerError</description></item>
    /// <item><description>502 - BadGateway</description></item>
    /// <item><description>503 - ServiceUnavailable</description></item>
    /// <item><description>504 - GatewayTimeout</description></item>
    /// <item><description>408 - RequestTimeout</description></item>
    /// <item><description>429 - TooManyRequests</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// These codes indicate transient failures that are safe to retry. 4xx client errors
    /// (except 408 and 429) are typically not retried as they indicate bad requests.
    /// Can be overridden per client via <see cref="ClientSettings.HttpRetryStatusCodes"/>.
    /// </para>
    /// </remarks>
    public List<int> HttpRetryStatusCodes { get; init; } =
    [
        500, // InternalServerError
        502, // BadGateway
        503, // ServiceUnavailable
        504, // GatewayTimeout
        408, // RequestTimeout
        429  // TooManyRequests
    ];

    /// <summary>
    /// Gets or initializes the default logging settings for all HTTP clients.
    /// </summary>
    /// <value>
    /// Logging configuration controlling request/response header and body logging.
    /// Default: headers logged, bodies not logged. Never null - initialized with safe defaults.
    /// </value>
    /// <remarks>
    /// <para>
    /// <b>Security Warning:</b> Request/response bodies may contain sensitive data (credentials, PII).
    /// Sensitive headers (Authorization, Cookie, tokens, keys, passwords) are automatically masked.
    /// </para>
    /// <para>
    /// <b>Performance Impact:</b> Logging bodies buffers entire content in memory. Use sparingly
    /// in production or enable only for specific clients.
    /// </para>
    /// <para>
    /// Can be overridden per client via <see cref="ClientSettings.Logging"/>.
    /// </para>
    /// </remarks>
    public LoggingSettings Logging { get; init; } = new();
}