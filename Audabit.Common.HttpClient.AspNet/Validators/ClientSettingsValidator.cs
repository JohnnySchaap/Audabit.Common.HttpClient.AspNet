using Audabit.Common.HttpClient.AspNet.Settings;
using FluentValidation;

namespace Audabit.Common.HttpClient.AspNet.Validators;

/// <summary>
/// Validator for <see cref="ClientSettings"/> configuration.
/// </summary>
/// <remarks>
/// <para>
/// This validator ensures client-specific settings are valid by:
/// <list type="bullet">
/// <item><description>Verifying <see cref="ClientSettings.BaseUrl"/> is not empty and is a valid absolute HTTP/HTTPS URL</description></item>
/// <item><description>Validating optional override settings (Timeout, Retry, CircuitBreaker) only when they are not null</description></item>
/// <item><description>Delegating to nested validators for comprehensive validation of override settings</description></item>
/// </list>
/// </para>
/// <para>
/// <b>URL Validation:</b> BaseUrl must be:
/// <list type="number">
/// <item><description>Not empty or whitespace</description></item>
/// <item><description>A valid absolute URI (parseable by Uri.TryCreate method)</description></item>
/// <item><description>Using HTTP or HTTPS scheme (other schemes like FTP are rejected)</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Override Pattern:</b> All nullable properties (Timeout, Retry, CircuitBreaker) use the <c>When()</c> guard
/// to only validate when the property is not null. This allows selective overriding of default settings
/// while ensuring overrides are valid when present.
/// </para>
/// </remarks>
public sealed class ClientSettingsValidator : AbstractValidator<ClientSettings>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClientSettingsValidator"/> class
    /// and configures validation rules for client settings.
    /// </summary>
    public ClientSettingsValidator()
    {
        RuleFor(x => x.BaseUrl)
            .NotEmpty()
            .WithMessage("Client BaseUrl must not be empty.")
            .Must(BeAValidUrl)
            .WithMessage("Client BaseUrl must be a valid URL.");

        When(x => x.Timeout != null, () => RuleFor(x => x.Timeout!)
                .SetValidator(new TimeoutSettingsValidator()));

        When(x => x.Retry != null, () => RuleFor(x => x.Retry!)
                .SetValidator(new RetrySettingsValidator()));

        When(x => x.CircuitBreaker != null, () => RuleFor(x => x.CircuitBreaker!)
                .SetValidator(new CircuitBreakerSettingsValidator()));
    }

    private static bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}