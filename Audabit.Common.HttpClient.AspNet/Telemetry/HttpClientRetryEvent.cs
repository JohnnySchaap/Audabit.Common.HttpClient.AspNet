using System.Diagnostics.CodeAnalysis;
using Audabit.Common.Observability.Events;

namespace Audabit.Common.HttpClient.AspNet.Telemetry;

/// <summary>
/// Event raised when an HTTP client retry policy executes a retry attempt.
/// </summary>
/// <remarks>
/// <para>
/// <b>Event Overview:</b>
/// This warning-level event is emitted before each retry attempt when a transient failure occurs,
/// providing visibility into retry behavior and helping diagnose intermittent service issues.
/// </para>
/// <para>
/// <b>When This Event Is Raised:</b>
/// <list type="bullet">
/// <item><description>When an HTTP request fails with a transient error (network timeout, 5xx status codes)</description></item>
/// <item><description>Before the delay period and subsequent retry attempt</description></item>
/// <item><description>For each retry up to the configured maximum retry attempts</description></item>
/// <item><description>Both for exception-based failures and HTTP status code failures</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Captured Information:</b>
/// The event captures the HTTP client name, current retry attempt number, delay before next retry,
/// and the reason for the retry (exception message or HTTP status code). This information helps
/// identify patterns in service failures and validate retry policy configuration.
/// </para>
/// <para>
/// <b>Integration Notes:</b>
/// This event integrates with the Audabit observability infrastructure and is automatically
/// emitted by <see cref="Factories.ResiliencePolicyFactory"/> when the retry policy is triggered.
/// The event is excluded from code coverage as it's a simple data container.
/// </para>
/// </remarks>
/// <example>
/// Typical log output when this event is raised:
/// <code>
/// {
///   "Timestamp": "2024-01-15T10:30:15Z",
///   "Level": "Warning",
///   "Event": "HttpClientRetryEvent",
///   "Properties": {
///     "clientName": "WeatherApiClient",
///     "retryCount": 2,
///     "delayMilliseconds": 400.0,
///     "reason": "Status code: 503"
///   }
/// }
/// </code>
/// </example>
[ExcludeFromCodeCoverage]
public sealed class HttpClientRetryEvent : LoggingEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientRetryEvent"/> class.
    /// </summary>
    /// <param name="clientName">The name of the HTTP client performing the retry.</param>
    /// <param name="retryCount">The current retry attempt number (1-based).</param>
    /// <param name="delayMilliseconds">The delay in milliseconds before the next retry attempt.</param>
    /// <param name="reason">The reason for the retry (exception message or HTTP status code), or "Unknown" if not specified.</param>
    /// <remarks>
    /// The event is automatically raised by the resilience policy factory when a retry is triggered.
    /// All parameters are added to the event's property bag for structured logging and telemetry.
    /// </remarks>
    public HttpClientRetryEvent(string clientName, int retryCount, double delayMilliseconds, string? reason)
        : base(nameof(HttpClientRetryEvent))
    {
        Properties.Add(nameof(clientName), clientName);
        Properties.Add(nameof(retryCount), retryCount);
        Properties.Add(nameof(delayMilliseconds), delayMilliseconds);
        Properties.Add(nameof(reason), reason ?? "Unknown");
    }
}