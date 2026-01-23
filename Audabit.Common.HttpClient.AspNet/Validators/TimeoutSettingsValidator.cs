using Audabit.Common.HttpClient.AspNet.Settings;
using FluentValidation;

namespace Audabit.Common.HttpClient.AspNet.Validators;

/// <summary>
/// Validator for <see cref="TimeoutSettings"/> configuration.
/// </summary>
/// <remarks>
/// <para>
/// This validator ensures timeout settings are within reasonable operational bounds:
/// <list type="bullet">
/// <item><description><see cref="TimeoutSettings.TimeoutSeconds"/>: 1-300 (1 second to 5 minutes)</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Rationale for Constraints:</b>
/// </para>
/// <para>
/// <b>Lower Bound (1 second):</b> Prevents zero or negative timeouts which would:
/// <list type="bullet">
/// <item><description>Cause immediate request failures</description></item>
/// <item><description>Never allow requests to complete</description></item>
/// <item><description>Create misleading timeout errors</description></item>
/// </list>
/// Minimum of 1 second allows for fast local/internal service calls.
/// </para>
/// <para>
/// <b>Upper Bound (300 seconds):</b> Maximum of 5 minutes prevents:
/// <list type="bullet">
/// <item><description>Excessively long-running requests that consume resources</description></item>
/// <item><description>Thread pool exhaustion from hanging requests</description></item>
/// <item><description>Poor user experience from minute-long waits</description></item>
/// <item><description>Masking underlying service performance issues</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Interaction with Retry Policies:</b> This timeout applies per request attempt, not total request time.
/// With retries enabled:
/// <list type="bullet">
/// <item><description>Each retry attempt gets the full timeout duration</description></item>
/// <item><description>Total time = (MaxRetryAttempts + 1) * TimeoutSeconds + retry delays</description></item>
/// <item><description>Example: 3 retries with 30s timeout = up to 120s+ total</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Polly Integration:</b> When Polly resilience policies are enabled, this timeout is managed by
/// Polly's timeout policy. When Polly is disabled, this value is set on <c>HttpClient.Timeout</c> directly.
/// </para>
/// </remarks>
public sealed class TimeoutSettingsValidator : AbstractValidator<TimeoutSettings>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeoutSettingsValidator"/> class
    /// and configures validation rules for timeout settings.
    /// </summary>
    public TimeoutSettingsValidator()
    {
        RuleFor(x => x.TimeoutSeconds)
            .GreaterThan(0)
            .WithMessage("TimeoutSeconds must be greater than 0.")
            .LessThanOrEqualTo(300)
            .WithMessage("TimeoutSeconds must be less than or equal to 300.");
    }
}