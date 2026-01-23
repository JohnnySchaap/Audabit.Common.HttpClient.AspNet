# Audabit.Common.HttpClient.AspNet

A resilient HTTP client library for ASP.NET Core applications that provides Polly-based retry and circuit breaker policies, request/response logging, correlation ID propagation, and comprehensive observability.

## Why You Should Use Resilient HTTP Clients

Calling external APIs without retry logic and circuit breakers is a recipe for fragile systems. Network issues happen, services go down temporarily, and transient errors occur constantly in distributed systems.

### Automatic Retry on Failures

Transient failures like network timeouts or temporary service unavailability should retry automatically. Without retry logic, your application fails every time there's a brief network hiccup. This library handles retries with exponential backoff, so temporary issues resolve themselves without manual intervention.

### Prevent Cascading Failures

When a downstream service is struggling, continuing to send requests makes it worse. Circuit breakers detect when a service is unhealthy and stop sending requests temporarily, giving it time to recover. Without this, your application can bring down already struggling services.

### Correlation ID Propagation

In distributed systems, you need to track requests across service boundaries. This library automatically propagates correlation IDs (W3C Trace Context format) from incoming requests to outgoing HTTP calls, maintaining the trace through your entire system without manual header management.

### Observability Built In

Understanding what your HTTP clients are doing is critical. This library provides request/response logging with sensitive header masking, timing metrics, and integration with your logging infrastructure—all configured once and applied consistently.

## Library Design Principles

> **ConfigureAwait Best Practices**: This library follows Microsoft's recommended async/await best practices by using `ConfigureAwait(false)` on all await statements. This eliminates unnecessary context switches and improves performance by allowing continuations to run on any thread pool thread rather than marshaling back to the original synchronization context.

## Features

- **Resilience Policies**: Built-in retry and circuit breaker patterns using Polly
- **Request/Response Logging**: Configurable logging with sensitive header masking
- **Correlation ID Propagation**: Automatic W3C Trace Context compliant correlation ID forwarding to downstream services
- **Observability Integration**: Structured logging through Audabit.Common.Observability
- **Flexible Configuration**: Per-client settings with default fallbacks
- **Typed Clients**: Generic client configuration with type-safe logging
- **FluentValidation**: Built-in settings validation
- **.NET 10.0 Support**: Built for the latest .NET framework

## Installation

### Via .NET CLI

```bash
dotnet add package Audabit.Common.HttpClient.AspNet
```

### Via Package Manager Console

```powershell
Install-Package Audabit.Common.HttpClient.AspNet
```

## Getting Started

### Basic Usage

Configure HTTP clients in your `appsettings.json`:

```json
{
  "HttpClientSettings": {
    "DefaultSettings": {
      "Timeout": {
        "TimeoutSeconds": 30
      },
      "Retry": {
        "MaxRetryAttempts": 3,
        "RetryDelayMilliseconds": 500,
        "RetryBackoffPower": 2
      },
      "CircuitBreaker": {
        "FailureThreshold": 0.5,
        "MinimumThroughput": 10,
        "SampleDurationMilliseconds": 60000,
        "DurationOfBreakMilliseconds": 30000
      },
      "HttpRetryStatusCodes": [500, 502, 503, 504, 408, 429],
      "Logging": {
        "LogRequest": false,
        "LogRequestHeaders": false,
        "LogResponse": false,
        "LogResponseHeaders": false
      }
    },
    "Clients": {
      "MyApiClient": {
        "BaseUrl": "https://api.example.com",
        "Timeout": {
          "TimeoutSeconds": 15
        },
        "Logging": {
          "LogRequest": true,
          "LogRequestHeaders": true,
          "LogResponse": true,
          "LogResponseHeaders": true
        }
      }
    }
  }
}
```

Register HTTP clients in `Program.cs`:

```csharp
using Audabit.Common.HttpClient.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Get HTTP client settings from configuration
var httpClientSettingsSection = builder.Configuration.GetSection(nameof(HttpClientSettings));

// Register resilient HTTP clients
builder.Services
    .AddResilientHttpClients(httpClientSettingsSection)
    .AddResilientHttpClient<MyApiClient>();

var app = builder.Build();
app.Run();
```

### Choosing Features à la Carte

You can select which features to enable for each HTTP client. Available features are:
- **Polly**: Retry and circuit breaker resilience policies
- **CorrelationId**: Automatic correlation ID propagation
- **Logging**: Request/response logging with structured events

```csharp
builder.Services
    .AddResilientHttpClients(httpClientSettingsSection)
    // All features (resilience + CorrelationId + Logging)
    .AddResilientHttpClient<MyFullyFeaturedClient>()
    
    // Basic HTTP client (no handlers or policies)
    .AddHttpClient<MyBasicClient>()
    
    // Only resilience policies (retry and circuit breaker)
    .AddHttpClientWithResilience<MyResilientClient>()
    
    // Only correlation ID propagation
    .AddHttpClientWithCorrelationId<MyTrackedClient>()
    
    // Only logging
    .AddHttpClientWithLogging<MyLoggedClient>()
    
    // Resilience + CorrelationId
    .AddHttpClientWithResilienceAndCorrelationId<MyResilientTrackedClient>()
    
    // Resilience + Logging
    .AddHttpClientWithResilienceAndLogging<MyResilientLoggedClient>()
    
    // CorrelationId + Logging
    .AddHttpClientWithCorrelationIdAndLogging<MyTrackedLoggedClient>();
```

**Note**: Even when using feature-specific methods, all clients still use settings from `HttpClientSettings` for timeouts, base URLs, and policy configuration.

### Using the HTTP Client

Inject the HTTP client into your services:

```csharp
public class MyService
{
    private readonly HttpClient _httpClient;
    
    public MyService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(nameof(MyApiClient));
    }
    
    public async Task<MyData> GetDataAsync()
    {
        var response = await _httpClient.GetAsync("/endpoint");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MyData>();
    }
}
```

### Custom Resilience Policy Callbacks

When registering HTTP clients with resilience policies, you can provide custom callbacks to handle policy events:

```csharp
builder.Services
    .AddResilientHttpClients(httpClientSettingsSection)
    .AddResilientHttpClient<MyApiClient>(
        onTimeout: async (context, timeout, task) =>
        {
            // Called when a request times out
            // context: Polly execution context
            // timeout: The timeout duration that was exceeded
            // task: The task that timed out
            await logger.LogWarningAsync($"Request timed out after {timeout.TotalSeconds} seconds");
        },
        onRetry: (outcome, timespan, retryCount, context) =>
        {
            // Called before each retry attempt
            // outcome: Contains the exception or HTTP response that triggered the retry
            // timespan: The delay before the next retry
            // retryCount: The current retry attempt number
            // context: Polly execution context
            logger.LogWarning($"Retry {retryCount} after {timespan.TotalMilliseconds}ms");
        },
        onBreak: (outcome, duration, context) =>
        {
            // Called when the circuit breaker opens (breaks)
            // outcome: Contains the exception or HTTP response that caused the break
            // duration: How long the circuit will remain open
            // context: Polly execution context
            logger.LogError($"Circuit breaker opened for {duration.TotalSeconds} seconds");
        },
        onReset: (context) =>
        {
            // Called when the circuit breaker resets to closed state
            logger.LogInformation("Circuit breaker reset to closed state");
        },
        onHalfOpen: () =>
        {
            // Called when the circuit breaker enters half-open state
            logger.LogInformation("Circuit breaker is half-open, testing if service recovered");
        });
```

**Notes**:
- Callbacks are optional - if not provided, default telemetry events are emitted
- onTimeout signature: `Func<Context, TimeSpan, Task, Task>?`
- onRetry signature: `Action<DelegateResult<HttpResponseMessage>, TimeSpan, int, Context>?`
- onBreak signature: `Action<DelegateResult<HttpResponseMessage>, TimeSpan, Context>?`
- onReset signature: `Action<Context>?`
- onHalfOpen signature: `Action?`

## Configuration

### Settings Structure

- **DefaultSettings**: Applied to all clients as base configuration
- **Clients**: Client-specific overrides (must include BaseUrl)

### Timeout Settings

```csharp
public class TimeoutSettings
{
    public int TimeoutSeconds { get; set; } = 30;
}
```

### Retry Settings

```csharp
public class RetrySettings
{
    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryDelayMilliseconds { get; set; } = 500;
    public int RetryBackoffPower { get; set; } = 1; // 1=linear, 2=exponential
}
```

### Circuit Breaker Settings

```csharp
public class CircuitBreakerSettings
{
    public double FailureThreshold { get; set; } = 1.0; // 0.0-1.0 (50% = 0.5)
    public int MinimumThroughput { get; set; } = 10;
    public int SampleDurationMilliseconds { get; set; } = 60000;
    public int DurationOfBreakMilliseconds { get; set; } = 10000;
}
```

### Logging Settings

```csharp
public class LoggingSettings
{
    public bool LogRequest { get; init; }          // Log request method and URI
    public bool LogRequestHeaders { get; init; }   // Log request headers (requires LogRequest=true)
    public bool LogRequestBody { get; init; }      // Log request body (requires LogRequest=true)
    public bool LogResponse { get; init; }         // Log response status code and timing
    public bool LogResponseHeaders { get; init; }  // Log response headers (requires LogResponse=true)
    public bool LogResponseBody { get; init; }     // Log response body (requires LogResponse=true)
}
```

**Configuration Example**:

```json
{
  "Logging": {
    "LogRequest": true,
    "LogRequestHeaders": true,
    "LogRequestBody": false,
    "LogResponse": true,
    "LogResponseHeaders": true,
    "LogResponseBody": true
  }
}
```

**Important Notes**:
- **Security Warning**: Request/response bodies may contain sensitive data (credentials, PII). Use body logging carefully in production.
- **Performance Impact**: Logging bodies buffers entire content in memory, which can impact performance for large payloads.
- **Hierarchical Settings**: Header and body logging only applies when parent LogRequest/LogResponse is enabled.
- Sensitive headers (Authorization, Cookie, tokens, keys, passwords) are automatically masked in logs.

## Features in Detail

### Timeout Handling

The library implements intelligent timeout handling based on whether Polly resilience is enabled:

**With Polly Resilience** (AddResilientHttpClient, AddHttpClientWithResilience, etc.):
- `HttpClient.Timeout` is set to `InfiniteTimeSpan`
- Timeout is managed by Polly's timeout policy
- Allows Polly to handle retry logic before timeout occurs
- Prevents timeout from interfering with retry attempts

**Without Polly Resilience** (AddHttpClient, AddHttpClientWithLogging, etc.):
- `HttpClient.Timeout` is set to the configured value from settings
- Standard HttpClient timeout behavior applies
- No retry or circuit breaker policies

```csharp
// Example: Client WITH Polly - timeout managed by Polly policy
builder.Services.AddResilientHttpClient<MyApiClient>();
// HttpClient.Timeout = InfiniteTimeSpan
// Polly timeout policy = 30 seconds (from config)

// Example: Client WITHOUT Polly - timeout on HttpClient
builder.Services.AddHttpClient<MyBasicClient>();
// HttpClient.Timeout = 30 seconds (from config)
```

This design ensures timeout behavior is consistent with the resilience strategy chosen for each client.

### Correlation ID Propagation

The `CorrelationIdDelegatingHandler` automatically adds the X-Correlation-Id header (W3C Trace Context format) to outgoing requests:

```csharp
// Automatically retrieves correlation ID from Activity.Current baggage
// and adds it to outgoing HTTP requests in W3C Trace Context format
// Format: 00-{traceId}-{spanId}-{flags}
```

### Request/Response Logging

The `HttpLoggingDelegatingHandler<T>` logs HTTP traffic with configurable detail:

- Request method, URI, body, and headers
- Response status code, body, and headers
- Sensitive header masking for security
- Structured logging via IEmitter<T>

### Resilience Policies

**Retry Policy**:
- Exponential backoff support
- Configurable retry attempts and delays
- Status code-based retry triggers
- Structured event logging

**Circuit Breaker**:
- Advanced circuit breaker with failure threshold
- Minimum throughput requirement
- Automatic circuit reset after break duration
- State transition logging

### Telemetry Events

All events are logged through `Audabit.Common.Observability`:

- `HttpClientTimeoutEvent` - Warning level (emitted when timeout policy is configured)
- `HttpClientRetryEvent` - Warning level (emitted on each retry attempt)
- `HttpClientCircuitBreakerOpenedEvent` - Error level (emitted when circuit opens)
- `HttpClientCircuitBreakerResetEvent` - Information level (emitted when circuit resets)
- `HttpClientRequestEvent` - Information level (emitted for each HTTP request when logging enabled)
- `HttpClientResponseEvent` - Information level (emitted for each HTTP response when logging enabled)
- `HttpClientRequestFailedEvent` - Error level (emitted when request fails)

## Dependencies

- `Polly` - Resilience and transient-fault-handling
- `Audabit.Common.Observability` - Structured logging
- `Audabit.Common.Validation.AspNet` - FluentValidation integration
- `FluentValidation` - Settings validation

## Related Packages

This library works seamlessly with other Audabit packages:

- **[Audabit.Common.CorrelationId.AspNet](https://dev.azure.com/johnnyschaap/Audabit/_artifacts/feed/Audabit/NuGet/Audabit.Common.CorrelationId.AspNet)**: Automatically propagates W3C Trace Context correlation IDs to downstream HTTP services
- **[Audabit.Common.Observability.AspNet](https://dev.azure.com/johnnyschaap/Audabit/_artifacts/feed/Audabit/NuGet/Audabit.Common.Observability.AspNet)**: Integrates HTTP client metrics and tracing with OpenTelemetry for comprehensive monitoring

While this package depends on Audabit.Common.Observability and Audabit.Common.Validation.AspNet, combining it with these packages provides a complete HTTP client solution.

## Build and Test

### Prerequisites

- .NET 10.0 SDK or later
- Visual Studio 2022 / VS Code / Rider (optional)

### Building

```bash
dotnet restore
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Creating NuGet Package

```bash
dotnet pack --configuration Release
```

## CI/CD Pipeline

This project uses Azure DevOps pipelines with the following features:

- **Automatic Versioning**: Major and minor versions from csproj, patch version from build number
- **Prerelease Builds**: Non-main branches create prerelease packages (e.g., `9.0.123-feature-auth`)
- **Code Formatting**: Enforces `dotnet format` standards
- **Code Coverage**: Generates and publishes code coverage reports
- **Automated Publishing**: Pushes packages to Azure Artifacts feed

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

### Development Guidelines

1. Follow existing code style and conventions
2. Ensure all tests pass before submitting PR
3. Add tests for new features
4. Update documentation as needed
5. Run `dotnet format` before committing

## License

Copyright © Audabit Software Solutions B.V. 2026

Licensed under the Apache License, Version 2.0. See [LICENSE](LICENSE) file for details.

## Authors

- [John Schaap](https://github.com/JohnnySchaap) - [Audabit Software Solutions B.V.](https://audabit.nl)

## Acknowledgments

- Built on [Polly](https://github.com/App-vNext/Polly) for resilience policies
- Designed for [ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet)
