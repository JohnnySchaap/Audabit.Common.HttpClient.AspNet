using System.Diagnostics;

namespace Audabit.Common.HttpClient.AspNet.Handlers;

/// <summary>
/// Delegating handler that automatically propagates correlation IDs across service boundaries
/// by adding the X-Correlation-Id header to outgoing HTTP requests.
/// </summary>
/// <remarks>
/// <para>
/// This handler enables distributed tracing by ensuring all outgoing HTTP requests include
/// the correlation ID from the current <see cref="Activity"/> context. This allows requests
/// to be traced across multiple microservices.
/// </para>
/// <para>
/// <b>How It Works:</b>
/// <list type="number">
/// <item><description>Retrieves the correlation ID from <see cref="Activity.Current"/> baggage</description></item>
/// <item><description>Checks if the X-Correlation-Id header is already present on the request</description></item>
/// <item><description>If not present, adds the header with the correlation ID value</description></item>
/// <item><description>Passes the request to the next handler in the pipeline</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Integration:</b> The correlation ID is typically set by middleware (e.g., CorrelationIdMiddleware)
/// at the start of the request pipeline and stored in <c>Activity.Current.Baggage</c>.
/// This handler ensures it's propagated to all downstream services.
/// </para>
/// <para>
/// <b>Thread Safety:</b> Uses <see cref="Activity.Current"/> which is ambient context (flows with async/await).
/// Safe for concurrent requests as each request has its own <see cref="Activity"/> context.
/// </para>
/// </remarks>
public sealed class CorrelationIdDelegatingHandler : DelegatingHandler
{
    private const string CorrelationIdHeaderName = "X-Correlation-Id";

    /// <summary>
    /// Sends an HTTP request after adding the X-Correlation-Id header if a correlation ID exists in the current activity context.
    /// </summary>
    /// <param name="request">The HTTP request message to be sent.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>The HTTP response message from the server.</returns>
    /// <remarks>
    /// <para>
    /// This method performs the following steps:
    /// <list type="number">
    /// <item><description>Retrieves the correlation ID from <see cref="Activity.Current"/> baggage with key "X-Correlation-Id"</description></item>
    /// <item><description>If a correlation ID exists and is not already present in request headers, adds it</description></item>
    /// <item><description>Invokes the next handler in the pipeline via <see cref="HttpMessageHandler.SendAsync"/></description></item>
    /// </list>
    /// </para>
    /// <para>
    /// If the request already has an X-Correlation-Id header (e.g., manually set), this method does not override it.
    /// This allows explicit correlation ID control when needed.
    /// </para>
    /// </remarks>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var correlationId = Activity.Current?.GetBaggageItem(CorrelationIdHeaderName);

        if (!string.IsNullOrWhiteSpace(correlationId)
            && !request.Headers.Contains(CorrelationIdHeaderName))
        {
            request.Headers.Add(CorrelationIdHeaderName, correlationId);
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}