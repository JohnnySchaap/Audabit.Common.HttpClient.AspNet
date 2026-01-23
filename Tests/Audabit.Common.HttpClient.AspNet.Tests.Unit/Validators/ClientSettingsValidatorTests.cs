using Audabit.Common.HttpClient.AspNet.Settings;
using Audabit.Common.HttpClient.AspNet.Validators;

namespace Audabit.Common.HttpClient.AspNet.Tests.Unit.Validators;

public class ClientSettingsValidatorTests
{
    private readonly ClientSettingsValidator _validator = new();

    [Theory]
    [InlineData("https://api.example.com")]
    [InlineData("http://localhost:5000")]
    [InlineData("https://api.example.com/v1")]
    public void GivenValidBaseUrl_ShouldPass(string baseUrl)
    {
        // Arrange
        var settings = new ClientSettings { BaseUrl = baseUrl };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void GivenEmptyBaseUrl_ShouldFail(string baseUrl)
    {
        // Arrange
        var settings = new ClientSettings { BaseUrl = baseUrl };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage.Contains("BaseUrl must not be empty"));
    }

    [Theory]
    [InlineData("ftp://example.com")]
    [InlineData("not-a-url")]
    [InlineData("www.example.com")]
    public void GivenInvalidBaseUrl_ShouldFail(string baseUrl)
    {
        // Arrange
        var settings = new ClientSettings { BaseUrl = baseUrl };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage.Contains("valid URL"));
    }

    [Fact]
    public void GivenClientWithNoOverrides_ShouldPass()
    {
        // Arrange
        var settings = new ClientSettings
        {
            BaseUrl = "https://api.example.com",
            Timeout = null,
            Retry = null,
            CircuitBreaker = null,
            Logging = null
        };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(60)]
    [InlineData(300)]
    public void GivenValidTimeoutOverride_ShouldPass(int timeoutSeconds)
    {
        // Arrange
        var settings = new ClientSettings
        {
            BaseUrl = "https://api.example.com",
            Timeout = new TimeoutSettings { TimeoutSeconds = timeoutSeconds }
        };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void GivenInvalidTimeoutOverride_ShouldFail()
    {
        // Arrange
        var settings = new ClientSettings
        {
            BaseUrl = "https://api.example.com",
            Timeout = new TimeoutSettings { TimeoutSeconds = 0 }
        };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage.Contains("TimeoutSeconds"));
    }

    [Fact]
    public void GivenValidRetryOverride_ShouldPass()
    {
        // Arrange
        var settings = new ClientSettings
        {
            BaseUrl = "https://api.example.com",
            Retry = new RetrySettings
            {
                MaxRetryAttempts = 5,
                RetryDelayMilliseconds = 1000,
                RetryBackoffPower = 2
            }
        };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void GivenInvalidRetryOverride_ShouldFail()
    {
        // Arrange
        var settings = new ClientSettings
        {
            BaseUrl = "https://api.example.com",
            Retry = new RetrySettings { MaxRetryAttempts = -1 }
        };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage.Contains("MaxRetryAttempts"));
    }

    [Fact]
    public void GivenValidCircuitBreakerOverride_ShouldPass()
    {
        // Arrange
        var settings = new ClientSettings
        {
            BaseUrl = "https://api.example.com",
            CircuitBreaker = new CircuitBreakerSettings
            {
                FailureThreshold = 0.5,
                MinimumThroughput = 5,
                SampleDurationMilliseconds = 5000,
                DurationOfBreakMilliseconds = 3000
            }
        };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void GivenInvalidCircuitBreakerOverride_ShouldFail()
    {
        // Arrange
        var settings = new ClientSettings
        {
            BaseUrl = "https://api.example.com",
            CircuitBreaker = new CircuitBreakerSettings { FailureThreshold = 0 }
        };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage.Contains("CircuitBreakerFailureThreshold"));
    }
}