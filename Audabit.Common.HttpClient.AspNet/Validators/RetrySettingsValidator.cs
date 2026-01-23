using Audabit.Common.HttpClient.AspNet.Settings;
using FluentValidation;

namespace Audabit.Common.HttpClient.AspNet.Validators;

/// <summary>
/// Validator for <see cref="RetrySettings"/> configuration.
/// </summary>
/// <remarks>
/// <para>
/// This validator ensures retry policy settings are within safe operational bounds:
/// <list type="bullet">
/// <item><description><see cref="RetrySettings.MaxRetryAttempts"/>: 0-10 (0 disables retries, upper limit prevents retry storms)</description></item>
/// <item><description><see cref="RetrySettings.RetryDelayMilliseconds"/>: 1-60000 (prevents excessively short or long delays)</description></item>
/// <item><description><see cref="RetrySettings.RetryBackoffPower"/>: 1-10 (controls backoff strategy from linear to aggressive exponential)</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Rationale for Constraints:</b>
/// </para>
/// <para>
/// <b>MaxRetryAttempts (0-10):</b> Upper limit of 10 prevents excessive retry storms that could:
/// <list type="bullet">
/// <item><description>Overwhelm already-struggling downstream services</description></item>
/// <item><description>Consume thread pool resources</description></item>
/// <item><description>Create cascading failures across service boundaries</description></item>
/// </list>
/// Setting to 0 allows complete disabling of retries for idempotency-sensitive operations.
/// </para>
/// <para>
/// <b>RetryDelayMilliseconds (1-60000):</b> Upper limit of 60 seconds prevents:
/// <list type="bullet">
/// <item><description>Excessively long wait times that could exceed timeout thresholds</description></item>
/// <item><description>Blocking threads for unreasonable durations</description></item>
/// <item><description>Poor user experience with minute-long delays</description></item>
/// </list>
/// Lower bound ensures meaningful delays between retries.
/// </para>
/// <para>
/// <b>RetryBackoffPower (1-10):</b> Controls exponential growth rate:
/// <list type="bullet">
/// <item><description>1 = linear backoff (delay, 2*delay, 3*delay, ...)</description></item>
/// <item><description>2 = exponential backoff (delay, 4*delay, 9*delay, ...)</description></item>
/// <item><description>Higher values create increasingly large gaps, giving services more recovery time</description></item>
/// </list>
/// Upper limit prevents astronomically large delays from very high powers.
/// </para>
/// </remarks>
public sealed class RetrySettingsValidator : AbstractValidator<RetrySettings>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RetrySettingsValidator"/> class
    /// and configures validation rules for retry settings.
    /// </summary>
    public RetrySettingsValidator()
    {
        RuleFor(x => x.MaxRetryAttempts)
            .GreaterThanOrEqualTo(0)
            .WithMessage("MaxRetryAttempts must be greater than or equal to 0.")
            .LessThanOrEqualTo(10)
            .WithMessage("MaxRetryAttempts must be less than or equal to 10.");

        RuleFor(x => x.RetryDelayMilliseconds)
            .GreaterThan(0)
            .WithMessage("RetryDelayMilliseconds must be greater than 0.")
            .LessThanOrEqualTo(60000)
            .WithMessage("RetryDelayMilliseconds must be less than or equal to 60000.");

        RuleFor(x => x.RetryBackoffPower)
            .GreaterThanOrEqualTo(1)
            .WithMessage("RetryBackoffPower must be greater than or equal to 1.")
            .LessThanOrEqualTo(10)
            .WithMessage("RetryBackoffPower must be less than or equal to 10.");
    }
}