namespace Audabit.Common.HttpClient.AspNet.Settings;

/// <summary>
/// Timeout settings for HTTP requests.
/// </summary>
/// <remarks>
/// <para>
/// The timeout applies per request attempt. When used with retry policies, each retry attempt
/// has the full timeout duration.
/// </para>
/// <para>
/// <b>Polly Integration:</b> When Polly resilience policies are enabled, the timeout is managed
/// by Polly's timeout policy and <c>HttpClient.Timeout</c> is set to <c>Timeout.InfiniteTimeSpan</c>.
/// When Polly is disabled, <c>HttpClient.Timeout</c> is set to this value.
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
///   "Timeout": {
///     "TimeoutSeconds": 60
///   }
/// }
/// </code>
/// 
/// Or using environment variables:
/// <code>
/// HttpClientSettings__DefaultSettings__Timeout__TimeoutSeconds=60
/// HttpClientSettings__Clients__PaymentApi__Timeout__TimeoutSeconds=120
/// </code>
/// </example>
public sealed record TimeoutSettings
{
    /// <summary>
    /// Gets or initializes the timeout duration in seconds for HTTP requests.
    /// </summary>
    /// <value>
    /// Valid range: 1-300 (1 second to 5 minutes). Default is 30 seconds.
    /// Must be greater than 0.
    /// </value>
    /// <remarks>
    /// <para>
    /// This timeout applies to each individual request attempt, not the total time including retries.
    /// With retries enabled, total request time = (MaxRetryAttempts + 1) * TimeoutSeconds + retry delays.
    /// </para>
    /// <para>
    /// <b>Example:</b> With 3 retry attempts and 30s timeout, total maximum time could be:
    /// (3 + 1) * 30s + retry delays = 120s+ (not including backoff delays).
    /// </para>
    /// <para>
    /// Upper limit of 300s prevents excessively long-running requests.
    /// Validated by <see cref="Validators.TimeoutSettingsValidator"/>.
    /// </para>
    /// </remarks>
    public int TimeoutSeconds { get; init; } = 30;
}