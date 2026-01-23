using Audabit.Common.HttpClient.AspNet.Settings;
using FluentValidation;

namespace Audabit.Common.HttpClient.AspNet.Validators;

/// <summary>
/// Validator for <see cref="CircuitBreakerSettings"/> configuration.
/// </summary>
/// <remarks>
/// <para>
/// This validator ensures circuit breaker settings are within safe operational bounds:
/// <list type="bullet">
/// <item><description><see cref="CircuitBreakerSettings.FailureThreshold"/>: 0.0-1.0 (0% to 100% failure rate)</description></item>
/// <item><description><see cref="CircuitBreakerSettings.MinimumThroughput"/>: > 0 (minimum requests before breaking)</description></item>
/// <item><description><see cref="CircuitBreakerSettings.SampleDurationMilliseconds"/>: > 0 (time window for measuring failures)</description></item>
/// <item><description><see cref="CircuitBreakerSettings.DurationOfBreakMilliseconds"/>: > 0 (how long circuit stays open)</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Rationale for Constraints:</b>
/// </para>
/// <para>
/// <b>FailureThreshold (0.0-1.0):</b> Percentage of failures that triggers circuit breaking:
/// <list type="bullet">
/// <item><description>0.0 (0%) = extremely sensitive, breaks on any failure (not recommended)</description></item>
/// <item><description>0.5 (50%) = breaks when half of requests fail (common threshold)</description></item>
/// <item><description>1.0 (100%) = never breaks (circuit breaker effectively disabled, default setting)</description></item>
/// </list>
/// Default of 1.0 requires explicit configuration to enable circuit breaking, preventing accidental activation.
/// </para>
/// <para>
/// <b>MinimumThroughput (> 0):</b> Prevents premature circuit breaking in low-traffic scenarios:
/// <list type="bullet">
/// <item><description>Without this, a single failure with no other requests = 100% failure rate</description></item>
/// <item><description>Ensures statistical significance before breaking the circuit</description></item>
/// <item><description>Typical values: 5-20 requests depending on traffic patterns</description></item>
/// </list>
/// </para>
/// <para>
/// <b>SampleDurationMilliseconds (> 0):</b> Sliding time window for failure rate calculation:
/// <list type="bullet">
/// <item><description>Shorter durations (e.g., 10-30s) react faster to failures but may be sensitive to temporary spikes</description></item>
/// <item><description>Longer durations (e.g., 60-120s) provide more stable detection but react slower</description></item>
/// <item><description>Should be longer than typical request completion times to avoid false positives</description></item>
/// </list>
/// </para>
/// <para>
/// <b>DurationOfBreakMilliseconds (> 0):</b> Recovery time before testing if service has recovered:
/// <list type="bullet">
/// <item><description>Too short: doesn't give downstream services time to recover, circuit re-breaks immediately</description></item>
/// <item><description>Too long: extends downtime unnecessarily if service has already recovered</description></item>
/// <item><description>Typical values: 10-30 seconds for most web services</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Circuit Breaker State Machine:</b>
/// <list type="number">
/// <item><description><b>Closed:</b> Normal operation, requests flow through normally</description></item>
/// <item><description><b>Open:</b> Failure threshold exceeded, all requests fail immediately for DurationOfBreakMilliseconds</description></item>
/// <item><description><b>Half-Open:</b> Testing recovery, one request is allowed through as a test</description></item>
/// <item><description>If test succeeds: transition to Closed; if test fails: return to Open for another DurationOfBreakMilliseconds</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class CircuitBreakerSettingsValidator : AbstractValidator<CircuitBreakerSettings>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CircuitBreakerSettingsValidator"/> class
    /// and configures validation rules for circuit breaker settings.
    /// </summary>
    public CircuitBreakerSettingsValidator()
    {
        RuleFor(x => x.FailureThreshold)
            .GreaterThan(0)
            .WithMessage("CircuitBreakerFailureThreshold must be greater than 0.")
            .LessThanOrEqualTo(1)
            .WithMessage("CircuitBreakerFailureThreshold must be less than or equal to 1.");

        RuleFor(x => x.MinimumThroughput)
            .GreaterThan(0)
            .WithMessage("CircuitBreakerMinimumThroughput must be greater than 0.");

        RuleFor(x => x.SampleDurationMilliseconds)
            .GreaterThan(0)
            .WithMessage("CircuitBreakerSampleDurationMilliseconds must be greater than 0.");

        RuleFor(x => x.DurationOfBreakMilliseconds)
            .GreaterThan(0)
            .WithMessage("CircuitBreakerDurationOfBreakMilliseconds must be greater than 0.");
    }
}