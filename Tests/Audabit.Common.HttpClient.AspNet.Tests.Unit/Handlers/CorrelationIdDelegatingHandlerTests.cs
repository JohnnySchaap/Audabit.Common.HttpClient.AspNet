using System.Diagnostics;
using System.Net;
using Audabit.Common.HttpClient.AspNet.Handlers;

namespace Audabit.Common.HttpClient.AspNet.Tests.Unit.Handlers;

public class CorrelationIdDelegatingHandlerTests
{
    private const string CorrelationIdHeaderName = "X-Correlation-Id";

    [Fact]
    public async Task SendAsync_WhenCorrelationIdExists_ShouldAddHeaderToRequest()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        var activity = new Activity("test");
        activity.AddBaggage(CorrelationIdHeaderName, correlationId);
        activity.Start();

        var handler = new CorrelationIdDelegatingHandler
        {
            InnerHandler = new TestHttpMessageHandler()
        };

        var client = new System.Net.Http.HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

        try
        {
            // Act
            await client.SendAsync(request);

            // Assert
            request.Headers.Contains(CorrelationIdHeaderName).ShouldBeTrue();
            request.Headers.GetValues(CorrelationIdHeaderName).First().ShouldBe(correlationId);
        }
        finally
        {
            activity.Stop();
        }
    }

    [Fact]
    public async Task SendAsync_WhenNoCorrelationId_ShouldNotAddHeader()
    {
        // Arrange
        var handler = new CorrelationIdDelegatingHandler
        {
            InnerHandler = new TestHttpMessageHandler()
        };

        var client = new System.Net.Http.HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

        // Act
        await client.SendAsync(request);

        // Assert
        request.Headers.Contains(CorrelationIdHeaderName).ShouldBeFalse();
    }

    [Fact]
    public async Task SendAsync_WhenHeaderAlreadyExists_ShouldNotOverwrite()
    {
        // Arrange
        var existingCorrelationId = "existing-id";
        var activityCorrelationId = "activity-id";

        var activity = new Activity("test");
        activity.AddBaggage(CorrelationIdHeaderName, activityCorrelationId);
        activity.Start();

        var handler = new CorrelationIdDelegatingHandler
        {
            InnerHandler = new TestHttpMessageHandler()
        };

        var client = new System.Net.Http.HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");
        request.Headers.Add(CorrelationIdHeaderName, existingCorrelationId);

        try
        {
            // Act
            await client.SendAsync(request);

            // Assert
            request.Headers.GetValues(CorrelationIdHeaderName).First().ShouldBe(existingCorrelationId);
        }
        finally
        {
            activity.Stop();
        }
    }

    [Fact]
    public async Task SendAsync_WhenCorrelationIdIsWhitespace_ShouldNotAddHeader()
    {
        // Arrange
        var activity = new Activity("test");
        activity.AddBaggage(CorrelationIdHeaderName, "   ");
        activity.Start();

        var handler = new CorrelationIdDelegatingHandler
        {
            InnerHandler = new TestHttpMessageHandler()
        };

        var client = new System.Net.Http.HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

        try
        {
            // Act
            await client.SendAsync(request);

            // Assert
            request.Headers.Contains(CorrelationIdHeaderName).ShouldBeFalse();
        }
        finally
        {
            activity.Stop();
        }
    }

    [Fact]
    public async Task SendAsync_ShouldReturnResponse()
    {
        // Arrange
        var handler = new CorrelationIdDelegatingHandler
        {
            InnerHandler = new TestHttpMessageHandler()
        };

        var client = new System.Net.Http.HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    private class TestHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}