using Audabit.Common.HttpClient.AspNet.Settings;
using Audabit.Common.HttpClient.AspNet.Validators;

namespace Audabit.Common.HttpClient.AspNet.Tests.Unit.Validators;

public class RetrySettingsValidatorTests
{
    private readonly RetrySettingsValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(10)]
    public void GivenValidMaxRetryAttempts_ShouldPass(int maxRetryAttempts)
    {
        // Arrange
        var settings = new RetrySettings { MaxRetryAttempts = maxRetryAttempts };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void GivenMaxRetryAttemptsLessThanZero_ShouldFail()
    {
        // Arrange
        var settings = new RetrySettings { MaxRetryAttempts = -1 };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(RetrySettings.MaxRetryAttempts));
    }

    [Fact]
    public void GivenMaxRetryAttemptsGreaterThan10_ShouldFail()
    {
        // Arrange
        var settings = new RetrySettings { MaxRetryAttempts = 11 };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(RetrySettings.MaxRetryAttempts));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(500)]
    [InlineData(60000)]
    public void GivenValidRetryDelayMilliseconds_ShouldPass(int retryDelayMilliseconds)
    {
        // Arrange
        var settings = new RetrySettings { RetryDelayMilliseconds = retryDelayMilliseconds };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GivenRetryDelayMillisecondsLessThanOrEqualToZero_ShouldFail(int retryDelayMilliseconds)
    {
        // Arrange
        var settings = new RetrySettings { RetryDelayMilliseconds = retryDelayMilliseconds };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(RetrySettings.RetryDelayMilliseconds));
    }

    [Fact]
    public void GivenRetryDelayMillisecondsGreaterThan60000_ShouldFail()
    {
        // Arrange
        var settings = new RetrySettings { RetryDelayMilliseconds = 60001 };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(RetrySettings.RetryDelayMilliseconds));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    public void GivenValidRetryBackoffPower_ShouldPass(int retryBackoffPower)
    {
        // Arrange
        var settings = new RetrySettings { RetryBackoffPower = retryBackoffPower };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GivenRetryBackoffPowerLessThanOne_ShouldFail(int retryBackoffPower)
    {
        // Arrange
        var settings = new RetrySettings { RetryBackoffPower = retryBackoffPower };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(RetrySettings.RetryBackoffPower));
    }

    [Fact]
    public void GivenRetryBackoffPowerGreaterThan10_ShouldFail()
    {
        // Arrange
        var settings = new RetrySettings { RetryBackoffPower = 11 };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(RetrySettings.RetryBackoffPower));
    }
}