using System.Diagnostics.CodeAnalysis;
using Audabit.Common.Observability.Events;

namespace Audabit.Common.HttpClient.AspNet.Telemetry;

/// <summary>
/// Event raised when a circuit breaker opens (transitions to open state) due to excessive failures.
/// </summary>
/// <remarks>
/// <para>
/// <b>Event Overview:</b>
/// This error-level event is emitted when the circuit breaker detects that the failure threshold
/// has been exceeded, preventing cascading failures by stopping requests to the failing service.
/// This is a critical operational event indicating a downstream service is experiencing problems.
/// </para>
/// <para>
/// <b>When This Event Is Raised:</b>
/// <list type="bullet">
/// <item><description>When the failure rate exceeds the configured failure threshold (e.g., 50%)</description></item>
/// <item><description>When minimum throughput requirement is met (prevents opening on low traffic)</description></item>
/// <item><description>At the moment the circuit transitions from closed to open state</description></item>
/// <item><description>Before the circuit enters the break duration period</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Captured Information:</b>
/// The event captures the HTTP client name, duration the circuit will remain open, and the reason
/// for opening (typically the last failure's exception or status code). This information is critical
/// for incident response and post-mortem analysis.
/// </para>
/// <para>
/// <b>Operational Impact:</b>
/// While the circuit is open, all requests to this client will fail immediately without attempting
/// to call the downstream service. This prevents resource exhaustion and gives the failing service
/// time to recover. After the break duration, the circuit enters half-open state to test recovery.
/// </para>
/// <para>
/// <b>Integration Notes:</b>
/// This event integrates with the Audabit observability infrastructure and is automatically
/// emitted by <see cref="Factories.ResiliencePolicyFactory"/> when the circuit breaker opens.
/// The event is excluded from code coverage as it's a simple data container.
/// </para>
/// </remarks>
/// <example>
/// Typical log output when this event is raised:
/// <code>
/// {
///   "Timestamp": "2024-01-15T10:30:20Z",
///   "Level": "Error",
///   "Event": "HttpClientCircuitBreakerOpenedEvent",
///   "Properties": {
///     "clientName": "WeatherApiClient",
///     "durationMilliseconds": 30000.0,
///     "reason": "Status code: 503"
///   }
/// }
/// </code>
/// </example>
[ExcludeFromCodeCoverage]
public sealed class HttpClientCircuitBreakerOpenedEvent : LoggingEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientCircuitBreakerOpenedEvent"/> class.
    /// </summary>
    /// <param name="clientName">The name of the HTTP client whose circuit breaker opened.</param>
    /// <param name="durationMilliseconds">The duration in milliseconds that the circuit will remain open before entering half-open state.</param>
    /// <param name="reason">The reason for opening the circuit (typically the last failure's exception message or HTTP status code), or "Unknown" if not specified.</param>
    /// <remarks>
    /// The event is automatically raised by the resilience policy factory when the circuit breaker opens.
    /// All parameters are added to the event's property bag for structured logging and telemetry.
    /// This is a critical operational event that should trigger alerts in production environments.
    /// </remarks>
    public HttpClientCircuitBreakerOpenedEvent(string clientName, double durationMilliseconds, string? reason)
        : base(nameof(HttpClientCircuitBreakerOpenedEvent))
    {
        Properties.Add(nameof(clientName), clientName);
        Properties.Add(nameof(durationMilliseconds), durationMilliseconds);
        Properties.Add(nameof(reason), reason ?? "Unknown");
    }
}