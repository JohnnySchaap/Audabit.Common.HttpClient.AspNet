namespace Audabit.Common.HttpClient.AspNet.Settings;

/// <summary>
/// Retry policy settings for handling transient failures in HTTP requests.
/// </summary>
/// <remarks>
/// <para>
/// The retry policy handles transient failures (network errors, server errors) by automatically
/// retrying failed requests with configurable delays and backoff strategies.
/// </para>
/// <para>
/// Retry behavior: delay = <see cref="RetryDelayMilliseconds"/> * (attempt ^ <see cref="RetryBackoffPower"/>)
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
///   "Retry": {
///     "MaxRetryAttempts": 5,
///     "RetryDelayMilliseconds": 1000,
///     "RetryBackoffPower": 2
///   }
/// }
/// </code>
/// 
/// This results in retry delays: 1s, 4s, 9s, 16s, 25s (exponential backoff).
/// 
/// Or using environment variables:
/// <code>
/// HttpClientSettings__DefaultSettings__Retry__MaxRetryAttempts=5
/// HttpClientSettings__DefaultSettings__Retry__RetryDelayMilliseconds=1000
/// HttpClientSettings__DefaultSettings__Retry__RetryBackoffPower=2
/// </code>
/// </example>
public sealed record RetrySettings
{
    /// <summary>
    /// Gets or initializes the maximum number of retry attempts after the initial request fails.
    /// </summary>
    /// <value>
    /// Valid range: 0-10. Default is 3.
    /// Set to 0 to disable retries completely.
    /// </value>
    /// <remarks>
    /// <para>
    /// Each retry attempt uses the full timeout configured in <see cref="TimeoutSettings"/>.
    /// Total request time = (MaxRetryAttempts + 1) * timeout + sum of retry delays.
    /// </para>
    /// <para>
    /// Upper limit of 10 prevents excessive retry storms that could overwhelm downstream services.
    /// Validated by <see cref="Validators.RetrySettingsValidator"/>.
    /// </para>
    /// </remarks>
    public int MaxRetryAttempts { get; init; } = 3;

    /// <summary>
    /// Gets or initializes the initial retry delay in milliseconds before the first retry attempt.
    /// </summary>
    /// <value>
    /// Valid range: 1-60000 (1ms to 60s). Default is 500ms.
    /// Must be greater than 0.
    /// </value>
    /// <remarks>
    /// <para>
    /// This is the base delay used in the backoff calculation:
    /// delay = RetryDelayMilliseconds * (attempt ^ <see cref="RetryBackoffPower"/>)
    /// </para>
    /// <para>
    /// For linear backoff (RetryBackoffPower=1): delays are 500ms, 1000ms, 1500ms, ...
    /// For exponential backoff (RetryBackoffPower=2): delays are 500ms, 2000ms, 4500ms, ...
    /// </para>
    /// <para>
    /// Upper limit prevents excessive wait times. Validated by <see cref="Validators.RetrySettingsValidator"/>.
    /// </para>
    /// </remarks>
    public int RetryDelayMilliseconds { get; init; } = 500;

    /// <summary>
    /// Gets or initializes the retry backoff power determining the backoff strategy.
    /// </summary>
    /// <value>
    /// Valid range: 1-10. Default is 1 (linear backoff).
    /// <list type="bullet">
    /// <item><description>1 = linear backoff (delay, 2*delay, 3*delay, ...)</description></item>
    /// <item><description>2 = exponential backoff (delay, 4*delay, 9*delay, ...)</description></item>
    /// <item><description>Higher values = more aggressive exponential backoff</description></item>
    /// </list>
    /// </value>
    /// <remarks>
    /// <para>
    /// The backoff formula: delay = <see cref="RetryDelayMilliseconds"/> * (attempt ^ RetryBackoffPower)
    /// </para>
    /// <para>
    /// Examples with RetryDelayMilliseconds=500:
    /// <list type="table">
    /// <listheader>
    /// <term>Power</term>
    /// <description>Retry Delays (attempts 1-4)</description>
    /// </listheader>
    /// <item><term>1</term><description>500ms, 1000ms, 1500ms, 2000ms</description></item>
    /// <item><term>2</term><description>500ms, 2000ms, 4500ms, 8000ms</description></item>
    /// <item><term>3</term><description>500ms, 4000ms, 13500ms, 32000ms</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Higher values create larger gaps between retries, giving downstream services more recovery time.
    /// Validated by <see cref="Validators.RetrySettingsValidator"/>.
    /// </para>
    /// </remarks>
    public int RetryBackoffPower { get; init; } = 1;
}