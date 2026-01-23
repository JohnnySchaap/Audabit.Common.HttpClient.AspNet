namespace Audabit.Common.HttpClient.AspNet.Settings;

/// <summary>
/// Circuit breaker policy settings for preventing cascading failures.
/// </summary>
/// <remarks>
/// <para>
/// The circuit breaker pattern monitors failure rates and "breaks" (stops sending requests)
/// when failures exceed the threshold, allowing downstream services time to recover.
/// </para>
/// <para>
/// Circuit breaker states:
/// <list type="bullet">
/// <item><description><b>Closed:</b> Normal operation, requests flow through</description></item>
/// <item><description><b>Open:</b> Circuit is broken, requests fail immediately without hitting the service</description></item>
/// <item><description><b>Half-Open:</b> Testing if service has recovered, one request is allowed through</description></item>
/// </list>
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
///   "CircuitBreaker": {
///     "FailureThreshold": 0.5,
///     "MinimumThroughput": 10,
///     "SampleDurationMilliseconds": 60000,
///     "DurationOfBreakMilliseconds": 10000
///   }
/// }
/// </code>
/// 
/// This breaks the circuit when 50% of requests (minimum 10) fail within a 60-second window.
/// The circuit stays open for 10 seconds before attempting to recover.
/// 
/// Or using environment variables:
/// <code>
/// HttpClientSettings__DefaultSettings__CircuitBreaker__FailureThreshold=0.5
/// HttpClientSettings__DefaultSettings__CircuitBreaker__MinimumThroughput=10
/// </code>
/// </example>
public sealed record CircuitBreakerSettings
{
    /// <summary>
    /// Gets or initializes the failure rate threshold (as a percentage) that triggers the circuit breaker.
    /// </summary>
    /// <value>
    /// Valid range: 0.0-1.0 (0% to 100%). Default is 1.0 (100%, effectively disabled).
    /// <list type="bullet">
    /// <item><description>0.5 = break when 50% of requests fail</description></item>
    /// <item><description>0.75 = break when 75% of requests fail</description></item>
    /// <item><description>1.0 = never break (circuit breaker disabled)</description></item>
    /// </list>
    /// </value>
    /// <remarks>
    /// <para>
    /// The circuit breaks when: (failures / total requests) >= FailureThreshold
    /// AND total requests >= <see cref="MinimumThroughput"/>
    /// within the <see cref="SampleDurationMilliseconds"/> window.
    /// </para>
    /// <para>
    /// Default of 1.0 effectively disables circuit breaking. Lower this value (e.g., 0.5)
    /// to enable failure detection. Validated by <see cref="Validators.CircuitBreakerSettingsValidator"/>.
    /// </para>
    /// </remarks>
    public double FailureThreshold { get; init; } = 1.0;

    /// <summary>
    /// Gets or initializes the minimum number of requests required before the circuit can break.
    /// </summary>
    /// <value>
    /// Valid range: 1-1000. Default is 10.
    /// Must be greater than 0.
    /// </value>
    /// <remarks>
    /// <para>
    /// This prevents the circuit from breaking on low-traffic scenarios where a single failure
    /// could cause a 100% failure rate. The circuit only breaks when BOTH the failure threshold
    /// AND minimum throughput criteria are met.
    /// </para>
    /// <para>
    /// <b>Example:</b> With FailureThreshold=0.5 and MinimumThroughput=10:
    /// - 5 failures out of 5 requests = circuit stays closed (below minimum throughput)
    /// - 6 failures out of 10 requests = circuit opens (60% failure rate, >= 10 requests)
    /// </para>
    /// <para>
    /// Validated by <see cref="Validators.CircuitBreakerSettingsValidator"/>.
    /// </para>
    /// </remarks>
    public int MinimumThroughput { get; init; } = 10;

    /// <summary>
    /// Gets or initializes the time window (in milliseconds) for measuring failure rates.
    /// </summary>
    /// <value>
    /// Valid range: 1000-300000 (1 second to 5 minutes). Default is 60000 (60 seconds).
    /// Must be greater than 0.
    /// </value>
    /// <remarks>
    /// <para>
    /// The circuit breaker calculates the failure rate using a sliding time window.
    /// Only requests within the last SampleDurationMilliseconds are considered.
    /// </para>
    /// <para>
    /// <b>Example:</b> With 60-second sample duration:
    /// - At 12:00:30, the circuit breaker considers requests from 11:59:30 to 12:00:30
    /// - Older requests outside this window are not counted
    /// </para>
    /// <para>
    /// Shorter durations react faster to failures but may be more sensitive to temporary spikes.
    /// Longer durations provide more stable failure detection but react slower.
    /// Validated by <see cref="Validators.CircuitBreakerSettingsValidator"/>.
    /// </para>
    /// </remarks>
    public int SampleDurationMilliseconds { get; init; } = 60000;

    /// <summary>
    /// Gets or initializes the duration (in milliseconds) the circuit stays open before attempting recovery.
    /// </summary>
    /// <value>
    /// Valid range: 1000-300000 (1 second to 5 minutes). Default is 10000 (10 seconds).
    /// Must be greater than 0.
    /// </value>
    /// <remarks>
    /// <para>
    /// When the circuit breaks (opens), it immediately rejects all requests for this duration,
    /// giving the downstream service time to recover. After this period, the circuit enters
    /// the Half-Open state and allows one test request through.
    /// </para>
    /// <para>
    /// State transition:
    /// <list type="number">
    /// <item><description>Circuit breaks (Closed → Open) when failure threshold exceeded</description></item>
    /// <item><description>All requests fail immediately for DurationOfBreakMilliseconds</description></item>
    /// <item><description>Circuit enters Half-Open state, allowing one test request</description></item>
    /// <item><description>If test succeeds: circuit closes (Half-Open → Closed)</description></item>
    /// <item><description>If test fails: circuit reopens for another DurationOfBreakMilliseconds</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Shorter durations attempt recovery faster but may not give services enough time to recover.
    /// Longer durations are more conservative but increase total downtime.
    /// Validated by <see cref="Validators.CircuitBreakerSettingsValidator"/>.
    /// </para>
    /// </remarks>
    public int DurationOfBreakMilliseconds { get; init; } = 10000;
}