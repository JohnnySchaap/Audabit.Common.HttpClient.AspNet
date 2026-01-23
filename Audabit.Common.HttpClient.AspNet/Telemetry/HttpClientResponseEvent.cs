using System.Diagnostics.CodeAnalysis;
using Audabit.Common.Observability.Events;

namespace Audabit.Common.HttpClient.AspNet.Telemetry;

/// <summary>
/// Event raised when an HTTP response is received from a downstream service.
/// </summary>
/// <remarks>
/// <para>
/// <b>Event Overview:</b>
/// This information-level event is emitted when the HTTP client receives a response, providing
/// visibility into downstream service behavior, response times, and data payloads for monitoring,
/// debugging, and audit purposes. The level of detail captured is controlled by logging settings.
/// </para>
/// <para>
/// <b>When This Event Is Raised:</b>
/// <list type="bullet">
/// <item><description>When LogResponse setting is enabled for the HTTP client</description></item>
/// <item><description>After a successful HTTP response is received from the downstream service</description></item>
/// <item><description>Before the response is returned to the calling code</description></item>
/// <item><description>For all HTTP status codes (2xx success, 4xx client errors, 5xx server errors)</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Captured Information:</b>
/// The event captures the HTTP client name, HTTP method, request URI, HTTP status code, and
/// optionally the response body and headers (based on LogResponseBody and LogResponseHeaders settings).
/// Sensitive headers are automatically masked to prevent credential leakage in logs.
/// </para>
/// <para>
/// <b>Security Considerations:</b>
/// Response bodies may contain PII or sensitive data. Use LogResponseBody setting carefully
/// in production environments. Headers containing tokens, session IDs, or other credentials
/// (Set-Cookie, Authorization) are automatically masked before logging.
/// </para>
/// <para>
/// <b>Integration Notes:</b>
/// This event integrates with the Audabit observability infrastructure and is automatically
/// emitted by <see cref="Handlers.HttpLoggingDelegatingHandler{T}"/> when response logging is enabled.
/// The event is excluded from code coverage as it's a simple data container.
/// </para>
/// </remarks>
/// <example>
/// Typical log output when this event is raised:
/// <code>
/// {
///   "Timestamp": "2024-01-15T10:30:01Z",
///   "Level": "Information",
///   "Event": "HttpClientResponseEvent",
///   "Properties": {
///     "clientName": "WeatherApiClient",
///     "method": "GET",
///     "uri": "https://api.weather.com/forecast",
///     "statusCode": 200,
///     "body": "{\"temperature\": 72, \"condition\": \"sunny\"}",
///     "headers": "Content-Type: application/json\nSet-Cookie: ***MASKED***"
///   }
/// }
/// </code>
/// </example>
[ExcludeFromCodeCoverage]
public sealed class HttpClientResponseEvent : LoggingEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientResponseEvent"/> class.
    /// </summary>
    /// <param name="clientName">The name of the HTTP client that received the response.</param>
    /// <param name="method">The HTTP method of the request (GET, POST, PUT, DELETE, etc.).</param>
    /// <param name="uri">The request URI (absolute URL).</param>
    /// <param name="statusCode">The HTTP status code returned (200, 404, 500, etc.).</param>
    /// <param name="body">The response body content, or null if not logged or empty. Only added to properties if not null/whitespace.</param>
    /// <param name="headers">The response headers as a formatted string, or null if not logged. Sensitive headers are masked. Only added to properties if not null/whitespace.</param>
    /// <remarks>
    /// The event is automatically raised by the HTTP logging handler when response logging is enabled.
    /// Body and headers are conditionally added to the property bag to avoid logging null/empty values.
    /// This follows the conditional property pattern to keep logs clean and relevant.
    /// </remarks>
    public HttpClientResponseEvent(string clientName, string method, string uri, int statusCode, string? body, string? headers)
        : base(nameof(HttpClientResponseEvent))
    {
        Properties.Add(nameof(clientName), clientName);
        Properties.Add(nameof(method), method);
        Properties.Add(nameof(uri), uri);
        Properties.Add(nameof(statusCode), statusCode);

        if (!string.IsNullOrWhiteSpace(body))
        {
            Properties.Add(nameof(body), body);
        }

        if (!string.IsNullOrWhiteSpace(headers))
        {
            Properties.Add(nameof(headers), headers);
        }
    }
}