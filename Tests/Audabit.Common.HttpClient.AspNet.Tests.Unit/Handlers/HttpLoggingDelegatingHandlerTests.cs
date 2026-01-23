using System.Net;
using Audabit.Common.HttpClient.AspNet.Handlers;

namespace Audabit.Common.HttpClient.AspNet.Tests.Unit.Handlers;

public class HttpLoggingDelegatingHandlerTests
{
    [Fact]
    public async Task SendAsync_WithNullEmitter_ShouldNotThrow()
    {
        // Arrange
        var handler = new HttpLoggingDelegatingHandler<TestClient>(
            () => null,
            logRequest: true,
            logResponse: true)
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

    [Fact]
    public async Task SendAsync_WithAllLoggingEnabled_ShouldSucceed()
    {
        // Arrange
        var handler = new HttpLoggingDelegatingHandler<TestClient>(
            () => null,
            logRequest: true,
            logRequestHeaders: true,
            logResponse: true,
            logResponseHeaders: true)
        {
            InnerHandler = new TestHttpMessageHandler()
        };

        var client = new System.Net.Http.HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.example.com/test")
        {
            Content = new StringContent("{\"test\":\"data\"}")
        };
        request.Headers.Add("X-Custom-Header", "test-value");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SendAsync_WithNoLogging_ShouldSucceed()
    {
        // Arrange
        var handler = new HttpLoggingDelegatingHandler<TestClient>(
            () => null,
            logRequest: false,
            logRequestHeaders: false,
            logResponse: false,
            logResponseHeaders: false)
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

    [Fact]
    public async Task SendAsync_WithSensitiveHeaders_ShouldSucceed()
    {
        // Arrange
        var handler = new HttpLoggingDelegatingHandler<TestClient>(
            () => null,
            logRequest: true,
            logRequestHeaders: true)
        {
            InnerHandler = new TestHttpMessageHandler()
        };

        var client = new System.Net.Http.HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");
        request.Headers.Add("Authorization", "Bearer secret-token");
        request.Headers.Add("X-API-Key", "api-key-value");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SendAsync_WhenExceptionOccurs_ShouldPropagateException()
    {
        // Arrange
        var handler = new HttpLoggingDelegatingHandler<TestClient>(
            () => null,
            logResponse: true)
        {
            InnerHandler = new FailingHttpMessageHandler()
        };

        var client = new System.Net.Http.HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(async () => await client.SendAsync(request));
    }

    private class TestClient { }

    private class TestHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"result\":\"success\"}")
            });
        }
    }

    private class FailingHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            throw new HttpRequestException("Simulated failure");
        }
    }
}