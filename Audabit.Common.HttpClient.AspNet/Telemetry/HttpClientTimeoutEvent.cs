using System.Diagnostics.CodeAnalysis;
using Audabit.Common.Observability.Events;

namespace Audabit.Common.HttpClient.AspNet.Telemetry;

/// <summary>
/// Event raised when an HTTP client timeout policy is configured.
/// </summary>
/// <remarks>
/// <para>
/// <b>Event Overview:</b>
/// This warning-level event is emitted when a timeout policy is created for an HTTP client,
/// providing visibility into timeout configuration for monitoring and troubleshooting.
/// </para>
/// <para>
/// <b>When This Event Is Raised:</b>
/// <list type="bullet">
/// <item><description>During application startup when HTTP clients are registered</description></item>
/// <item><description>For each HTTP client that has timeout policy configured</description></item>
/// <item><description>Both for Polly-managed and non-Polly timeout configurations</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Captured Information:</b>
/// The event captures the HTTP client name and the configured timeout duration in seconds.
/// This information can be used to verify timeout settings are correctly applied from configuration.
/// </para>
/// <para>
/// <b>Integration Notes:</b>
/// This event integrates with the Audabit observability infrastructure and is automatically
/// emitted by <see cref="Factories.ResiliencePolicyFactory"/> during policy creation.
/// The event is excluded from code coverage as it's a simple data container.
/// </para>
/// </remarks>
/// <example>
/// Typical log output when this event is raised:
/// <code>
/// {
///   "Timestamp": "2024-01-15T10:30:00Z",
///   "Level": "Warning",
///   "Event": "HttpClientTimeoutEvent",
///   "Properties": {
///     "clientName": "WeatherApiClient",
///     "timeoutSeconds": 30.0
///   }
/// }
/// </code>
/// </example>
[ExcludeFromCodeCoverage]
public sealed class HttpClientTimeoutEvent : LoggingEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientTimeoutEvent"/> class.
    /// </summary>
    /// <param name="clientName">The name of the HTTP client for which the timeout is configured.</param>
    /// <param name="timeoutSeconds">The timeout duration in seconds.</param>
    /// <remarks>
    /// The event is automatically raised by the resilience policy factory when creating
    /// timeout policies. Both parameters are added to the event's property bag for
    /// structured logging and telemetry.
    /// </remarks>
    public HttpClientTimeoutEvent(string clientName, double timeoutSeconds)
        : base(nameof(HttpClientTimeoutEvent))
    {
        Properties.Add(nameof(clientName), clientName);
        Properties.Add(nameof(timeoutSeconds), timeoutSeconds);
    }
}