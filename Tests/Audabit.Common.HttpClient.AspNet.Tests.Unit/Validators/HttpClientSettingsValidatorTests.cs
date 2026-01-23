using Audabit.Common.HttpClient.AspNet.Settings;
using Audabit.Common.HttpClient.AspNet.Validators;

namespace Audabit.Common.HttpClient.AspNet.Tests.Unit.Validators;

public class HttpClientSettingsValidatorTests
{
    private readonly HttpClientSettingsValidator _validator = new();

    private static HttpClientSettings CreateValidSettings()
    {
        return new HttpClientSettings
        {
            DefaultSettings = new DefaultSettings
            {
                Timeout = new TimeoutSettings { TimeoutSeconds = 30 },
                Retry = new RetrySettings { MaxRetryAttempts = 3, RetryDelayMilliseconds = 1000, RetryBackoffPower = 2 },
                CircuitBreaker = new CircuitBreakerSettings
                {
                    FailureThreshold = 0.5,
                    MinimumThroughput = 10,
                    SampleDurationMilliseconds = 60000,
                    DurationOfBreakMilliseconds = 30000
                },
                HttpRetryStatusCodes = [500, 502, 503],
                Logging = new LoggingSettings
                {
                    LogRequest = true,
                    LogRequestHeaders = false,
                    LogResponse = true,
                    LogResponseHeaders = false
                }
            },
            Clients = new Dictionary<string, ClientSettings>
            {
                ["TestClient"] = new ClientSettings { BaseUrl = "https://api.example.com" }
            }
        };
    }

    [Fact]
    public void Validate_WhenSettingsAreValid_ShouldReturnValid()
    {
        // Arrange
        var settings = CreateValidSettings();

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WhenDefaultSettingsIsNull_ShouldReturnInvalid()
    {
        // Arrange
        var settings = CreateValidSettings() with { DefaultSettings = null! };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage.Contains("DefaultSettings"));
    }

    [Fact]
    public void Validate_WhenClientsIsNull_ShouldReturnInvalid()
    {
        // Arrange
        var settings = CreateValidSettings() with { Clients = null! };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage.Contains("Clients"));
    }

    [Fact]
    public void Validate_WhenClientHasEmptyBaseUrl_ShouldReturnInvalid()
    {
        // Arrange
        var settings = CreateValidSettings() with
        {
            Clients = new Dictionary<string, ClientSettings>
            {
                ["TestClient"] = new ClientSettings { BaseUrl = "" }
            }
        };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage.Contains("BaseUrl"));
    }

    [Fact]
    public void Validate_WhenClientHasInvalidUrl_ShouldReturnInvalid()
    {
        // Arrange
        var settings = CreateValidSettings() with
        {
            Clients = new Dictionary<string, ClientSettings>
            {
                ["TestClient"] = new ClientSettings { BaseUrl = "not-a-valid-url" }
            }
        };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage.Contains("valid URL"));
    }
}