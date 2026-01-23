namespace Audabit.Common.HttpClient.AspNet.Settings;

/// <summary>
/// Client-specific settings that override default HTTP client configurations.
/// </summary>
/// <remarks>
/// <para>
/// Each property in this class is nullable, allowing selective overrides of <see cref="DefaultSettings"/>.
/// Only non-null properties will override the default values.
/// </para>
/// <para>
/// The <see cref="BaseUrl"/> is required and should be a valid absolute URI.
/// All other properties are optional overrides.
/// </para>
/// </remarks>
/// <example>
/// Configuration for a specific client:
/// <code>
/// {
///   "Clients": {
///     "PaymentApi": {
///       "BaseUrl": "https://payment.example.com",
///       "Timeout": { "TimeoutSeconds": 60 },
///       "Retry": {
///         "MaxRetryAttempts": 5,
///         "RetryDelayMilliseconds": 1000,
///         "RetryBackoffPower": 2
///       },
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
/// </example>
public sealed record ClientSettings
{
    /// <summary>
    /// Gets or initializes the base URL for the HTTP client.
    /// </summary>
    /// <value>
    /// A valid absolute URI (e.g., "https://api.example.com").
    /// Must start with http:// or https://. Empty string by default (must be configured).
    /// </value>
    /// <remarks>
    /// This is the only required property. All HTTP requests will use this as the base URL.
    /// Validated by <see cref="Validators.ClientSettingsValidator"/> to ensure proper URI format.
    /// </remarks>
    public string BaseUrl { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes the timeout settings for this specific client.
    /// </summary>
    /// <value>
    /// Timeout configuration, or null to use <see cref="DefaultSettings.Timeout"/>.
    /// </value>
    /// <remarks>
    /// When null, the client uses the timeout from <see cref="DefaultSettings"/>.
    /// When set, this overrides the default timeout for this client only.
    /// </remarks>
    public TimeoutSettings? Timeout { get; init; }

    /// <summary>
    /// Gets or initializes the retry settings for this specific client.
    /// </summary>
    /// <value>
    /// Retry policy configuration, or null to use <see cref="DefaultSettings.Retry"/>.
    /// </value>
    /// <remarks>
    /// When null, the client uses the retry settings from <see cref="DefaultSettings"/>.
    /// When set, this completely overrides the default retry configuration.
    /// </remarks>
    public RetrySettings? Retry { get; init; }

    /// <summary>
    /// Gets or initializes the circuit breaker settings for this specific client.
    /// </summary>
    /// <value>
    /// Circuit breaker policy configuration, or null to use <see cref="DefaultSettings.CircuitBreaker"/>.
    /// </value>
    /// <remarks>
    /// When null, the client uses the circuit breaker settings from <see cref="DefaultSettings"/>.
    /// When set, this overrides the default circuit breaker configuration.
    /// </remarks>
    public CircuitBreakerSettings? CircuitBreaker { get; init; }

    /// <summary>
    /// Gets or initializes the HTTP status codes that trigger retry and circuit breaker policies.
    /// </summary>
    /// <value>
    /// A list of HTTP status codes (100-599), or null to use <see cref="DefaultSettings.HttpRetryStatusCodes"/>.
    /// </value>
    /// <remarks>
    /// When null, the client uses the HTTP retry status codes from <see cref="DefaultSettings"/>.
    /// Common status codes: 500 (InternalServerError), 502 (BadGateway), 503 (ServiceUnavailable),
    /// 504 (GatewayTimeout), 408 (RequestTimeout), 429 (TooManyRequests).
    /// </remarks>
    public List<int>? HttpRetryStatusCodes { get; init; }

    /// <summary>
    /// Gets or initializes the logging settings for this specific client.
    /// </summary>
    /// <value>
    /// Logging configuration controlling request/response header and body logging,
    /// or null to use <see cref="DefaultSettings.Logging"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// When null, the client uses the logging settings from <see cref="DefaultSettings"/>.
    /// </para>
    /// <para>
    /// <b>Security Warning:</b> Enabling body logging may expose sensitive data.
    /// Use with caution and ensure sensitive headers are masked.
    /// </para>
    /// </remarks>
    public LoggingSettings? Logging { get; init; }
}