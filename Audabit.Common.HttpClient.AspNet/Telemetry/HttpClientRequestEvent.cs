using System.Diagnostics.CodeAnalysis;
using Audabit.Common.Observability.Events;

namespace Audabit.Common.HttpClient.AspNet.Telemetry;

/// <summary>
/// Event raised when an HTTP request is sent to a downstream service.
/// </summary>
/// <remarks>
/// <para>
/// <b>Event Overview:</b>
/// This information-level event is emitted when the HTTP client sends a request, providing
/// visibility into outbound HTTP traffic for monitoring, debugging, and audit purposes.
/// The level of detail captured is controlled by logging settings configuration.
/// </para>
/// <para>
/// <b>When This Event Is Raised:</b>
/// <list type="bullet">
/// <item><description>When LogRequest setting is enabled for the HTTP client</description></item>
/// <item><description>Before the request is sent to the downstream service</description></item>
/// <item><description>After correlation ID and other handler processing is complete</description></item>
/// <item><description>For each HTTP request (GET, POST, PUT, DELETE, etc.)</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Captured Information:</b>
/// The event captures the HTTP client name, HTTP method, request URI, and optionally the request
/// body and headers (based on LogRequestBody and LogRequestHeaders settings). Sensitive headers
/// are automatically masked to prevent credential leakage in logs.
/// </para>
/// <para>
/// <b>Security Considerations:</b>
/// Request bodies may contain sensitive data (credentials, PII). Use LogRequestBody setting
/// carefully in production environments. Headers containing tokens, keys, passwords, or other
/// credentials are automatically masked before logging.
/// </para>
/// <para>
/// <b>Integration Notes:</b>
/// This event integrates with the Audabit observability infrastructure and is automatically
/// emitted by <see cref="Handlers.HttpLoggingDelegatingHandler{T}"/> when request logging is enabled.
/// The event is excluded from code coverage as it's a simple data container.
/// </para>
/// </remarks>
/// <example>
/// Typical log output when this event is raised:
/// <code>
/// {
///   "Timestamp": "2024-01-15T10:30:00Z",
///   "Level": "Information",
///   "Event": "HttpClientRequestEvent",
///   "Properties": {
///     "clientName": "WeatherApiClient",
///     "method": "GET",
///     "uri": "https://api.weather.com/forecast",
///     "headers": "Accept: application/json\nAuthorization: ***MASKED***"
///   }
/// }
/// </code>
/// </example>
[ExcludeFromCodeCoverage]
public sealed class HttpClientRequestEvent : LoggingEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientRequestEvent"/> class.
    /// </summary>
    /// <param name="clientName">The name of the HTTP client sending the request.</param>
    /// <param name="method">The HTTP method (GET, POST, PUT, DELETE, etc.).</param>
    /// <param name="uri">The request URI (absolute URL).</param>
    /// <param name="body">The request body content, or null if not logged or empty. Only added to properties if not null/whitespace.</param>
    /// <param name="headers">The request headers as a formatted string, or null if not logged. Sensitive headers are masked. Only added to properties if not null/whitespace.</param>
    /// <remarks>
    /// The event is automatically raised by the HTTP logging handler when request logging is enabled.
    /// Body and headers are conditionally added to the property bag to avoid logging null/empty values.
    /// This follows the conditional property pattern to keep logs clean and relevant.
    /// </remarks>
    public HttpClientRequestEvent(string clientName, string method, string uri, string? body, string? headers)
        : base(nameof(HttpClientRequestEvent))
    {
        Properties.Add(nameof(clientName), clientName);
        Properties.Add(nameof(method), method);
        Properties.Add(nameof(uri), uri);

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