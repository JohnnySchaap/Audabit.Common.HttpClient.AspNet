using Audabit.Common.HttpClient.AspNet.Settings;
using Audabit.Common.HttpClient.AspNet.Validators;

namespace Audabit.Common.HttpClient.AspNet.Tests.Unit.Validators;

public class DefaultSettingsValidatorTests
{
    private readonly DefaultSettingsValidator _validator = new();

    [Fact]
    public void GivenValidSettings_ShouldPass()
    {
        // Arrange
        var settings = new DefaultSettings
        {
            Timeout = new TimeoutSettings { TimeoutSeconds = 30 },
            Retry = new RetrySettings
            {
                MaxRetryAttempts = 3,
                RetryDelayMilliseconds = 500,
                RetryBackoffPower = 2
            },
            CircuitBreaker = new CircuitBreakerSettings
            {
                FailureThreshold = 0.5,
                MinimumThroughput = 10,
                SampleDurationMilliseconds = 10000,
                DurationOfBreakMilliseconds = 5000
            },
            Logging = new LoggingSettings
            {
                LogRequest = true,
                LogResponse = true
            }
        };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void GivenNullTimeout_ShouldFail()
    {
        // Arrange
        var settings = new DefaultSettings
        {
            Timeout = null!,
            Retry = new RetrySettings(),
            CircuitBreaker = new CircuitBreakerSettings(),
            Logging = new LoggingSettings()
        };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(DefaultSettings.Timeout));
    }

    [Fact]
    public void GivenNullRetry_ShouldFail()
    {
        // Arrange
        var settings = new DefaultSettings
        {
            Timeout = new TimeoutSettings(),
            Retry = null!,
            CircuitBreaker = new CircuitBreakerSettings(),
            Logging = new LoggingSettings()
        };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(DefaultSettings.Retry));
    }

    [Fact]
    public void GivenNullCircuitBreaker_ShouldFail()
    {
        // Arrange
        var settings = new DefaultSettings
        {
            Timeout = new TimeoutSettings(),
            Retry = new RetrySettings(),
            CircuitBreaker = null!,
            Logging = new LoggingSettings()
        };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(DefaultSettings.CircuitBreaker));
    }

    [Fact]
    public void GivenNullLogging_ShouldFail()
    {
        // Arrange
        var settings = new DefaultSettings
        {
            Timeout = new TimeoutSettings(),
            Retry = new RetrySettings(),
            CircuitBreaker = new CircuitBreakerSettings(),
            Logging = null!
        };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(DefaultSettings.Logging));
    }

    [Fact]
    public void GivenInvalidNestedTimeout_ShouldFail()
    {
        // Arrange
        var settings = new DefaultSettings
        {
            Timeout = new TimeoutSettings { TimeoutSeconds = 0 },
            Retry = new RetrySettings(),
            CircuitBreaker = new CircuitBreakerSettings(),
            Logging = new LoggingSettings()
        };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName.Contains("Timeout"));
    }

    [Fact]
    public void GivenInvalidNestedRetry_ShouldFail()
    {
        // Arrange
        var settings = new DefaultSettings
        {
            Timeout = new TimeoutSettings(),
            Retry = new RetrySettings { MaxRetryAttempts = -1 },
            CircuitBreaker = new CircuitBreakerSettings(),
            Logging = new LoggingSettings()
        };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName.Contains("Retry"));
    }

    [Fact]
    public void GivenInvalidNestedCircuitBreaker_ShouldFail()
    {
        // Arrange
        var settings = new DefaultSettings
        {
            Timeout = new TimeoutSettings(),
            Retry = new RetrySettings(),
            CircuitBreaker = new CircuitBreakerSettings { FailureThreshold = 0 },
            Logging = new LoggingSettings()
        };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName.Contains("CircuitBreaker"));
    }
}