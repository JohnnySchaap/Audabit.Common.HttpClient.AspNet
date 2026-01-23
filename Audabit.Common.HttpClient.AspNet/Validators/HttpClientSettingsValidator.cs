using Audabit.Common.HttpClient.AspNet.Settings;
using FluentValidation;

namespace Audabit.Common.HttpClient.AspNet.Validators;

/// <summary>
/// Validator for <see cref="HttpClientSettings"/> configuration.
/// </summary>
/// <remarks>
/// <para>
/// This validator ensures the hierarchical HTTP client configuration is valid by:
/// <list type="bullet">
/// <item><description>Verifying <see cref="HttpClientSettings.DefaultSettings"/> is not null and valid</description></item>
/// <item><description>Ensuring <see cref="HttpClientSettings.Clients"/> dictionary is not null</description></item>
/// <item><description>Validating each client configuration in the <see cref="HttpClientSettings.Clients"/> dictionary</description></item>
/// </list>
/// </para>
/// <para>
/// The validator uses FluentValidation's <c>When()</c> guard pattern to safely handle nullable client
/// collections, preventing NullReferenceException during validation.
/// </para>
/// <para>
/// All nested settings (Timeout, Retry, CircuitBreaker) are validated recursively using their
/// respective validators to ensure comprehensive validation coverage.
/// </para>
/// </remarks>
public sealed class HttpClientSettingsValidator : AbstractValidator<HttpClientSettings>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientSettingsValidator"/> class.
    /// </summary>
    public HttpClientSettingsValidator()
    {
        RuleFor(x => x.DefaultSettings)
            .NotNull()
            .WithMessage("DefaultSettings must not be null.")
            .SetValidator(new DefaultSettingsValidator());

        RuleFor(x => x.Clients)
            .NotNull()
            .WithMessage("Clients must not be null.");

        When(x => x.Clients != null, () => RuleForEach(x => x.Clients.Values)
                .SetValidator(new ClientSettingsValidator()));
    }
}