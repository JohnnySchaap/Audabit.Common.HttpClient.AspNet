using System.Diagnostics.CodeAnalysis;
using Audabit.Common.Observability.Events;

namespace Audabit.Common.HttpClient.AspNet.Telemetry;

/// <summary>
/// Event raised when an HTTP request fails with an exception.
/// </summary>
/// <remarks>
/// <para>
/// <b>Event Overview:</b>
/// This error-level event is emitted when an HTTP request fails due to an unhandled exception,
/// such as network errors, timeouts, DNS failures, or connection refusals. This event provides
/// critical diagnostic information for troubleshooting service communication failures.
/// </para>
/// <para>
/// <b>When This Event Is Raised:</b>
/// <list type="bullet">
/// <item><description>When an HTTP request throws an exception (HttpRequestException, TaskCanceledException, etc.)</description></item>
/// <item><description>Before retry logic is applied (if retry policy is configured)</description></item>
/// <item><description>After all resilience policies have been exhausted without success</description></item>
/// <item><description>For fatal errors that cannot be recovered through retries</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Captured Information:</b>
/// The event captures the HTTP client name, HTTP method, request URI, exception details, and
/// optionally the request body and headers (for debugging purposes). Sensitive headers are
/// automatically masked to prevent credential leakage in error logs.
/// </para>
/// <para>
/// <b>Common Failure Scenarios:</b>
/// <list type="bullet">
/// <item><description>Network connectivity issues (DNS resolution, routing problems)</description></item>
/// <item><description>Timeout exceptions (request exceeded configured timeout)</description></item>
/// <item><description>SSL/TLS certificate validation failures</description></item>
/// <item><description>Connection refused (service not running or port blocked)</description></item>
/// <item><description>Socket exceptions (connection reset, broken pipe)</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Integration Notes:</b>
/// This event integrates with the Audabit observability infrastructure and is automatically
/// emitted by <see cref="Handlers.HttpLoggingDelegatingHandler{T}"/> when a request exception occurs.
/// The event is excluded from code coverage as it's a simple data container.
/// </para>
/// </remarks>
/// <example>
/// Typical log output when this event is raised:
/// <code>
/// {
///   "Timestamp": "2024-01-15T10:30:05Z",
///   "Level": "Error",
///   "Event": "HttpClientRequestFailedEvent",
///   "Properties": {
///     "clientName": "WeatherApiClient",
///     "method": "GET",
///     "uri": "https://api.weather.com/forecast",
///     "headers": "Accept: application/json\nAuthorization: ***MASKED***"
///   },
///   "Exception": "System.Net.Http.HttpRequestException: No such host is known."
/// }
/// </code>
/// </example>
[ExcludeFromCodeCoverage]
public sealed class HttpClientRequestFailedEvent : ErrorEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientRequestFailedEvent"/> class.
    /// </summary>
    /// <param name="clientName">The name of the HTTP client that encountered the failure.</param>
    /// <param name="method">The HTTP method of the failed request (GET, POST, PUT, DELETE, etc.).</param>
    /// <param name="uri">The request URI (absolute URL) that failed.</param>
    /// <param name="body">The request body content, or null if not available. Only added to properties if not null/whitespace.</param>
    /// <param name="headers">The request headers as a formatted string, or null if not available. Sensitive headers are masked. Only added to properties if not null/whitespace.</param>
    /// <param name="exception">The exception that caused the request to fail.</param>
    /// <remarks>
    /// The event is automatically raised by the HTTP logging handler when a request exception occurs.
    /// Body and headers are conditionally added to the property bag to avoid logging null/empty values.
    /// The exception message and full exception details are passed to the base ErrorEvent for structured logging.
    /// This event should trigger alerts in production environments as it indicates a service communication failure.
    /// </remarks>
    public HttpClientRequestFailedEvent(string clientName, string method, string uri, string? body, string? headers, Exception exception)
        : base(nameof(HttpClientRequestFailedEvent), exception.Message, exception)
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