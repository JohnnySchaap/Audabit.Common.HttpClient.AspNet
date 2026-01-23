namespace Audabit.Common.HttpClient.AspNet.Settings;

/// <summary>
/// Configuration settings for HTTP clients with resilience policies, correlation ID propagation, and logging.
/// </summary>
/// <remarks>
/// <para>
/// This class provides hierarchical configuration where <see cref="DefaultSettings"/> applies to all clients,
/// and individual <see cref="Clients"/> can override specific settings.
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
///       "HttpRetryStatusCodes": [500, 502, 503, 504, 408, 429]
///     },
///     "Clients": {
///       "WeatherApi": {
///         "BaseUrl": "https://api.weather.com",
///         "Timeout": { "TimeoutSeconds": 60 }
///       },
///       "PaymentApi": {
///         "BaseUrl": "https://payment.example.com",
///         "Retry": { "MaxRetryAttempts": 5 }
///       }
///     }
///   }
/// }
/// </code>
/// 
/// Or using environment variables:
/// <code>
/// HttpClientSettings__DefaultSettings__Timeout__TimeoutSeconds=30
/// HttpClientSettings__Clients__WeatherApi__BaseUrl=https://api.weather.com
/// HttpClientSettings__Clients__WeatherApi__Timeout__TimeoutSeconds=60
/// </code>
/// </example>
public sealed record HttpClientSettings
{
    /// <summary>
    /// Gets or initializes the default settings applied to all HTTP clients.
    /// </summary>
    /// <value>
    /// A <see cref="Settings.DefaultSettings"/> instance containing timeout, retry, circuit breaker,
    /// HTTP retry status codes, and logging configurations that apply to all clients unless overridden.
    /// Never null - initialized with sensible defaults.
    /// </value>
    /// <remarks>
    /// These settings serve as the baseline for all HTTP clients. Individual clients in the
    /// <see cref="Clients"/> dictionary can override any of these settings.
    /// </remarks>
    public DefaultSettings DefaultSettings { get; init; } = new();

    /// <summary>
    /// Gets or initializes the dictionary of individual client configurations keyed by client name.
    /// </summary>
    /// <value>
    /// A dictionary where the key is the HTTP client name (used in registration) and the value
    /// contains client-specific settings. Empty by default.
    /// </value>
    /// <remarks>
    /// Client-specific settings override <see cref="DefaultSettings"/>. Only non-null properties
    /// in a <see cref="ClientSettings"/> instance will override the defaults.
    /// </remarks>
    public Dictionary<string, ClientSettings> Clients { get; init; } = [];
}