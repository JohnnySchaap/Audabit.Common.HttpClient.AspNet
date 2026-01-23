using System.Text;
using Audabit.Common.HttpClient.AspNet.Telemetry;
using Audabit.Common.Observability.Emitters;
using Audabit.Common.Observability.Extensions;

namespace Audabit.Common.HttpClient.AspNet.Handlers;

/// <summary>
/// Delegating handler that logs HTTP requests and responses with configurable detail levels and automatic sensitive data masking.
/// </summary>
/// <typeparam name="T">The type parameter used to identify the HTTP client for logging purposes.</typeparam>
/// <remarks>
/// <para>
/// This handler provides comprehensive HTTP request/response logging with:
/// <list type="bullet">
/// <item><description><b>Selective Logging:</b> Separate flags for request/response, headers/bodies</description></item>
/// <item><description><b>Security:</b> Automatic masking of sensitive headers (Authorization, Cookie, tokens, keys, passwords)</description></item>
/// <item><description><b>Performance:</b> Cached type name to avoid repeated reflection on each request</description></item>
/// <item><description><b>Error Tracking:</b> Automatic logging of failed requests with exception details</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Security Warnings:</b>
/// <list type="bullet">
/// <item><description>Request/response bodies may contain sensitive data (credentials, PII, tokens)</description></item>
/// <item><description>Sensitive headers are automatically masked with "***" (Authorization, Cookie, API keys, etc.)</description></item>
/// <item><description>Body logging should be used sparingly in production or limited to non-sensitive endpoints</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Performance Considerations:</b>
/// <list type="bullet">
/// <item><description>Logging bodies buffers entire content in memory via <c>ReadAsStringAsync</c></description></item>
/// <item><description>For large payloads, this may increase memory pressure and latency</description></item>
/// <item><description>Client type name (typeof(T).Name) is cached in a field to avoid reflection overhead</description></item>
/// <item><description>Early returns prevent unnecessary work when logging is disabled</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Event Integration:</b> Raises telemetry events via <see cref="IEmitter{T}"/>:
/// <list type="bullet">
/// <item><description><see cref="HttpClientRequestEvent"/> (Information) - Logged before sending request</description></item>
/// <item><description><see cref="HttpClientResponseEvent"/> (Information) - Logged after receiving response</description></item>
/// <item><description><see cref="HttpClientRequestFailedEvent"/> (Error) - Logged when request throws exception</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Type Parameter Usage:</b> The generic type parameter <typeparamref name="T"/> is used solely for
/// identifying the HTTP client in logs. The actual type doesn't matter - it's the name that's logged.
/// Common pattern: use the API client interface type (e.g., <c>IWeatherApiClient</c>).
/// </para>
/// </remarks>
/// <param name="getEmitter">Factory function to retrieve the emitter for logging events.</param>
/// <param name="logRequest">Whether to log the request body and route.</param>
/// <param name="logRequestHeaders">Whether to log request headers (only if logRequest is true).</param>
/// <param name="logRequestBody">Whether to log request body (only if logRequest is true).</param>
/// <param name="logResponse">Whether to log the response.</param>
/// <param name="logResponseHeaders">Whether to log response headers (only if logResponse is true).</param>
/// <param name="logResponseBody">Whether to log response body (only if logResponse is true).</param>
public sealed class HttpLoggingDelegatingHandler<T>(
    Func<IEmitter<T>?> getEmitter,
    bool logRequest = false,
    bool logRequestHeaders = false,
    bool logRequestBody = false,
    bool logResponse = false,
    bool logResponseHeaders = false,
    bool logResponseBody = false) : DelegatingHandler
{
    private readonly bool _logRequest = logRequest;
    private readonly bool _logRequestHeaders = logRequest && logRequestHeaders;
    private readonly bool _logRequestBody = logRequest && logRequestBody;
    private readonly bool _logResponse = logResponse;
    private readonly bool _logResponseHeaders = logResponse && logResponseHeaders;
    private readonly bool _logResponseBody = logResponse && logResponseBody;
    private readonly string _clientName = typeof(T).Name; // Cache to avoid repeated reflection

    /// <summary>
    /// Sends an HTTP request and logs the request/response details based on configuration flags.
    /// </summary>
    /// <param name="request">The HTTP request message to be sent.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>The HTTP response message from the server.</returns>
    /// <remarks>
    /// <para>
    /// Execution flow:
    /// <list type="number">
    /// <item><description>If request logging is enabled, reads request body and headers (conditionally)</description></item>
    /// <item><description>Logs <see cref="HttpClientRequestEvent"/> with captured request details</description></item>
    /// <item><description>Sends the request via <see cref="HttpMessageHandler.SendAsync"/></description></item>
    /// <item><description>If successful and response logging enabled, reads response body/headers and logs <see cref="HttpClientResponseEvent"/></description></item>
    /// <item><description>If exception occurs, logs <see cref="HttpClientRequestFailedEvent"/> with exception details and re-throws</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Body Reading:</b> Request/response bodies are read using <c>ReadAsStringAsync</c>, which:
    /// <list type="bullet">
    /// <item><description>Buffers entire content in memory (can impact performance for large payloads)</description></item>
    /// <item><description>Respects the <c>cancellationToken</c> for async cancellation</description></item>
    /// <item><description>Returns null if logging is disabled or content is null</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var requestBody = await GetRequestBodyAsync(request, cancellationToken).ConfigureAwait(false);
        var requestHeaders = GetRequestHeaders(request);

        if (_logRequest)
        {
            LogRequest(request, requestBody, requestHeaders);
        }

        try
        {
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (_logResponse)
            {
                var responseBody = await GetResponseBodyAsync(response, cancellationToken).ConfigureAwait(false);
                var responseHeaders = GetResponseHeaders(response);
                LogResponse(request, response, responseBody, responseHeaders);
            }

            return response;
        }
        catch (Exception ex)
        {
            var emitter = getEmitter();
            emitter?.RaiseError(new HttpClientRequestFailedEvent(
                clientName: _clientName,
                method: request.Method.Method,
                uri: request.RequestUri?.ToString() ?? string.Empty,
                body: requestBody,
                headers: requestHeaders,
                exception: ex));

            throw;
        }
    }

    private async Task<string?> GetRequestBodyAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!_logRequestBody || request.Content == null)
        {
            return null;
        }

        return await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<string?> GetResponseBodyAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!_logResponseBody || response.Content == null)
        {
            return null;
        }

        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    private string? GetRequestHeaders(HttpRequestMessage request)
    {
        if (!_logRequestHeaders)
        {
            return null;
        }

        return FormatHeaders(request.Headers, request.Content?.Headers);
    }

    private string? GetResponseHeaders(HttpResponseMessage response)
    {
        if (!_logResponseHeaders)
        {
            return null;
        }

        return FormatHeaders(response.Headers, response.Content?.Headers);
    }

    private void LogRequest(HttpRequestMessage request, string? body, string? headers)
    {
        var emitter = getEmitter();
        emitter?.RaiseInformation(new HttpClientRequestEvent(
            clientName: _clientName,
            method: request.Method.Method,
            uri: request.RequestUri?.ToString() ?? string.Empty,
            body: body,
            headers: headers));
    }

    private void LogResponse(HttpRequestMessage request, HttpResponseMessage response, string? body, string? headers)
    {
        var emitter = getEmitter();
        emitter?.RaiseInformation(new HttpClientResponseEvent(
            clientName: _clientName,
            method: request.Method.Method,
            uri: request.RequestUri?.ToString() ?? string.Empty,
            statusCode: (int)response.StatusCode,
            body: body,
            headers: headers));
    }

    private static string? FormatHeaders(
        System.Net.Http.Headers.HttpHeaders? headers,
        System.Net.Http.Headers.HttpContentHeaders? contentHeaders = null)
    {
        if (headers == null && contentHeaders == null)
        {
            return null;
        }

        var sb = new StringBuilder();

        if (headers != null)
        {
            foreach (var header in headers)
            {
                var value = IsSensitiveHeader(header.Key)
                    ? "***"
                    : string.Join(", ", header.Value);
                sb.AppendLine($"{header.Key}: {value}");
            }
        }

        if (contentHeaders != null)
        {
            foreach (var header in contentHeaders)
            {
                var value = IsSensitiveHeader(header.Key)
                    ? "***"
                    : string.Join(", ", header.Value);
                sb.AppendLine($"{header.Key}: {value}");
            }
        }

        return sb.Length > 0 ? sb.ToString().TrimEnd() : null;
    }

    /// <summary>
    /// Determines if an HTTP header contains sensitive information that should be masked in logs.
    /// </summary>
    /// <param name="headerName">The name of the header to check.</param>
    /// <returns>True if the header is sensitive and should be masked; otherwise, false.</returns>
    /// <remarks>
    /// <para>
    /// This method uses a two-tier detection strategy:
    /// <list type="number">
    /// <item><description><b>Exact matches:</b> Standard sensitive headers (Authorization, Cookie, Set-Cookie, Proxy-Authorization, WWW-Authenticate)</description></item>
    /// <item><description><b>Pattern matches:</b> Custom headers containing sensitive keywords (token, key, secret, password, auth, credential)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Examples of masked headers:</b>
    /// <list type="bullet">
    /// <item><description>Authorization</description></item>
    /// <item><description>Cookie</description></item>
    /// <item><description>X-API-Key</description></item>
    /// <item><description>Bearer-Token</description></item>
    /// <item><description>X-Auth-Token</description></item>
    /// <item><description>X-Secret-Value</description></item>
    /// <item><description>Password</description></item>
    /// <item><description>Credentials</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Comparison is case-insensitive using <see cref="string.ToLowerInvariant"/> to ensure consistent
    /// detection regardless of header name casing.
    /// </para>
    /// </remarks>
    private static bool IsSensitiveHeader(string headerName)
    {
        var normalizedName = headerName.ToLowerInvariant();

        // Exact matches
        if (normalizedName is "authorization" or "cookie" or "set-cookie" or "proxy-authorization" or "www-authenticate")
        {
            return true;
        }

        // Pattern matches for common sensitive header patterns
        return normalizedName.Contains("token") ||
               normalizedName.Contains("key") ||
               normalizedName.Contains("secret") ||
               normalizedName.Contains("password") ||
               normalizedName.Contains("auth") ||
               normalizedName.Contains("credential");
    }
}