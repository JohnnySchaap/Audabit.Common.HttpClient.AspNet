using Audabit.Common.HttpClient.AspNet.Settings;
using FluentValidation;

namespace Audabit.Common.HttpClient.AspNet.Validators;

/// <summary>
/// Validator for <see cref="DefaultSettings"/> configuration.
/// </summary>
/// <remarks>
/// <para>
/// This validator ensures the default configuration applied to all HTTP clients is complete and valid by:
/// <list type="bullet">
/// <item><description>Verifying all required nested settings (Timeout, Retry, CircuitBreaker, Logging) are not null</description></item>
/// <item><description>Delegating to specialized validators for each nested settings type</description></item>
/// <item><description>Ensuring a valid baseline configuration exists before any client-specific overrides</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Non-Nullable Requirements:</b> Unlike <see cref="ClientSettings"/> where nested properties are nullable
/// to allow selective overrides, all properties in <see cref="DefaultSettings"/> must be non-null because
/// they serve as the fallback configuration for all HTTP clients.
/// </para>
/// <para>
/// This validator is executed at application startup via the Options validation pipeline, ensuring
/// misconfiguration is detected early before any HTTP requests are made.
/// </para>
/// </remarks>
public sealed class DefaultSettingsValidator : AbstractValidator<DefaultSettings>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultSettingsValidator"/> class
    /// and configures validation rules for default HTTP client settings.
    /// </summary>
    public DefaultSettingsValidator()
    {
        RuleFor(x => x.Timeout)
            .NotNull()
            .WithMessage("DefaultSettings.Timeout must not be null.")
            .SetValidator(new TimeoutSettingsValidator());

        RuleFor(x => x.Retry)
            .NotNull()
            .WithMessage("DefaultSettings.Retry must not be null.")
            .SetValidator(new RetrySettingsValidator());

        RuleFor(x => x.CircuitBreaker)
            .NotNull()
            .WithMessage("DefaultSettings.CircuitBreaker must not be null.")
            .SetValidator(new CircuitBreakerSettingsValidator());

        RuleFor(x => x.Logging)
            .NotNull()
            .WithMessage("DefaultSettings.Logging must not be null.");
    }
}