using System.Diagnostics.CodeAnalysis;
using Audabit.Common.Observability.Events;

namespace Audabit.Common.HttpClient.AspNet.Telemetry;

/// <summary>
/// Event raised when a circuit breaker resets (transitions to closed state) after successful recovery.
/// </summary>
/// <remarks>
/// <para>
/// <b>Event Overview:</b>
/// This information-level event is emitted when the circuit breaker successfully transitions from
/// half-open to closed state, indicating that the downstream service has recovered and normal
/// operations can resume. This is a positive operational event signaling the end of an incident.
/// </para>
/// <para>
/// <b>When This Event Is Raised:</b>
/// <list type="bullet">
/// <item><description>When the circuit is in half-open state and a test request succeeds</description></item>
/// <item><description>At the moment the circuit transitions from half-open to closed state</description></item>
/// <item><description>After the break duration has elapsed and recovery is confirmed</description></item>
/// <item><description>When normal request flow can resume without restrictions</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Captured Information:</b>
/// The event captures the HTTP client name, indicating which client's circuit breaker has recovered.
/// This information helps track service recovery times and circuit breaker effectiveness.
/// </para>
/// <para>
/// <b>Operational Impact:</b>
/// Once the circuit is closed, all requests to this client will be attempted normally. The failure
/// threshold counter is reset, and the circuit breaker will monitor for new failures. This event
/// typically corresponds to the resolution of an incident with the downstream service.
/// </para>
/// <para>
/// <b>Integration Notes:</b>
/// This event integrates with the Audabit observability infrastructure and is automatically
/// emitted by <see cref="Factories.ResiliencePolicyFactory"/> when the circuit breaker resets.
/// The event is excluded from code coverage as it's a simple data container.
/// </para>
/// </remarks>
/// <example>
/// Typical log output when this event is raised:
/// <code>
/// {
///   "Timestamp": "2024-01-15T10:31:00Z",
///   "Level": "Information",
///   "Event": "HttpClientCircuitBreakerResetEvent",
///   "Properties": {
///     "clientName": "WeatherApiClient"
///   }
/// }
/// </code>
/// </example>
[ExcludeFromCodeCoverage]
public sealed class HttpClientCircuitBreakerResetEvent : LoggingEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientCircuitBreakerResetEvent"/> class.
    /// </summary>
    /// <param name="clientName">The name of the HTTP client whose circuit breaker has reset to closed state.</param>
    /// <remarks>
    /// The event is automatically raised by the resilience policy factory when the circuit breaker resets.
    /// The client name is added to the event's property bag for structured logging and telemetry.
    /// This event indicates successful recovery from a downstream service failure.
    /// </remarks>
    public HttpClientCircuitBreakerResetEvent(string clientName)
        : base(nameof(HttpClientCircuitBreakerResetEvent))
    {
        Properties.Add(nameof(clientName), clientName);
    }
}