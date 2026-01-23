namespace Audabit.Common.HttpClient.AspNet.Settings;

/// <summary>
/// Configuration settings for HTTP client request and response logging.
/// </summary>
/// <remarks>
/// <para>
/// <b>Settings Overview:</b>
/// Controls granular logging of HTTP requests and responses for HTTP clients.
/// These settings allow you to balance observability needs with performance and security considerations.
/// </para>
/// <para>
/// <b>Hierarchical Configuration:</b>
/// Logging settings are hierarchical - headers and body logging only applies when the parent
/// LogRequest or LogResponse flag is enabled. This prevents unnecessary overhead when request/response
/// logging is disabled.
/// </para>
/// <para>
/// <b>Security Considerations:</b>
/// <list type="bullet">
/// <item><description>Request/response bodies may contain sensitive data (credentials, PII)</description></item>
/// <item><description>Headers are automatically masked if they contain sensitive information (Authorization, API keys, etc.)</description></item>
/// <item><description>Consider using LogRequest/LogResponse without body logging in production</description></item>
/// <item><description>Full body logging is best suited for development and troubleshooting</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Performance Impact:</b>
/// Logging request/response bodies buffers the entire content in memory and can impact
/// performance for large payloads. Use body logging selectively for high-volume endpoints.
/// </para>
/// <para>
/// All settings can be configured via appsettings.json or environment variables.
/// Environment variables use double underscore (__) as the hierarchy separator.
/// All settings default to false (no logging) for security and performance.
/// </para>
/// </remarks>
/// <example>
/// Configuration in appsettings.json:
/// <code>
/// {
///   "HttpClient": {
///     "Clients": {
///       "WeatherApiClient": {
///         "BaseUrl": "https://api.weather.com",
///         "Logging": {
///           "LogRequest": true,
///           "LogRequestHeaders": true,
///           "LogRequestBody": false,
///           "LogResponse": true,
///           "LogResponseHeaders": true,
///           "LogResponseBody": true
///         }
///       }
///     }
///   }
/// }
/// </code>
/// 
/// Or using environment variables:
/// <code>
/// HttpClient__Clients__WeatherApiClient__Logging__LogRequest=true
/// HttpClient__Clients__WeatherApiClient__Logging__LogRequestHeaders=true
/// HttpClient__Clients__WeatherApiClient__Logging__LogRequestBody=false
/// HttpClient__Clients__WeatherApiClient__Logging__LogResponse=true
/// HttpClient__Clients__WeatherApiClient__Logging__LogResponseHeaders=true
/// HttpClient__Clients__WeatherApiClient__Logging__LogResponseBody=true
/// </code>
/// </example>
public sealed record LoggingSettings
{
    /// <summary>
    /// Gets or initializes whether to log HTTP requests.
    /// </summary>
    /// <value>
    /// True to enable request logging (URI, method, timestamp); false to disable.
    /// Default is false.
    /// </value>
    /// <remarks>
    /// When enabled, logs basic request information. This is the parent flag for
    /// <see cref="LogRequestHeaders"/> and <see cref="LogRequestBody"/>.
    /// Even when false, request failures and retries are still logged.
    /// </remarks>
    public bool LogRequest { get; init; }

    /// <summary>
    /// Gets or initializes whether to log request headers.
    /// </summary>
    /// <value>
    /// True to include request headers in logs; false to exclude.
    /// Default is false.
    /// Only applies when <see cref="LogRequest"/> is true.
    /// </value>
    /// <remarks>
    /// Sensitive headers (Authorization, API keys, etc.) are automatically masked
    /// to prevent credential leakage. See <see cref="Handlers.HttpLoggingDelegatingHandler{T}"/>
    /// for masking logic.
    /// </remarks>
    public bool LogRequestHeaders { get; init; }

    /// <summary>
    /// Gets or initializes whether to log request body content.
    /// </summary>
    /// <value>
    /// True to include request body in logs; false to exclude.
    /// Default is false.
    /// Only applies when <see cref="LogRequest"/> is true.
    /// </value>
    /// <remarks>
    /// <para>
    /// <b>Performance Warning:</b>
    /// Logging request bodies buffers the entire content in memory before sending.
    /// This can impact performance for large payloads or high-volume endpoints.
    /// </para>
    /// <para>
    /// <b>Security Warning:</b>
    /// Request bodies may contain credentials, PII, or other sensitive data.
    /// Use this setting carefully in production environments.
    /// </para>
    /// </remarks>
    public bool LogRequestBody { get; init; }

    /// <summary>
    /// Gets or initializes whether to log HTTP responses.
    /// </summary>
    /// <value>
    /// True to enable response logging (status code, timestamp, duration); false to disable.
    /// Default is false.
    /// </value>
    /// <remarks>
    /// When enabled, logs basic response information. This is the parent flag for
    /// <see cref="LogResponseHeaders"/> and <see cref="LogResponseBody"/>.
    /// Even when false, response failures are still logged.
    /// </remarks>
    public bool LogResponse { get; init; }

    /// <summary>
    /// Gets or initializes whether to log response headers.
    /// </summary>
    /// <value>
    /// True to include response headers in logs; false to exclude.
    /// Default is false.
    /// Only applies when <see cref="LogResponse"/> is true.
    /// </value>
    /// <remarks>
    /// Sensitive headers (Set-Cookie, Authorization, etc.) are automatically masked
    /// to prevent credential leakage. See <see cref="Handlers.HttpLoggingDelegatingHandler{T}"/>
    /// for masking logic.
    /// </remarks>
    public bool LogResponseHeaders { get; init; }

    /// <summary>
    /// Gets or initializes whether to log response body content.
    /// </summary>
    /// <value>
    /// True to include response body in logs; false to exclude.
    /// Default is false.
    /// Only applies when <see cref="LogResponse"/> is true.
    /// </value>
    /// <remarks>
    /// <para>
    /// <b>Performance Warning:</b>
    /// Logging response bodies buffers the entire content in memory before reading.
    /// This can impact performance for large payloads or high-volume endpoints.
    /// </para>
    /// <para>
    /// <b>Security Warning:</b>
    /// Response bodies may contain PII or other sensitive data.
    /// Use this setting carefully in production environments.
    /// </para>
    /// </remarks>
    public bool LogResponseBody { get; init; }
}