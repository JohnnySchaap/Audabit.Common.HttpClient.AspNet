using Audabit.Common.HttpClient.AspNet.Settings;
using Audabit.Common.HttpClient.AspNet.Validators;

namespace Audabit.Common.HttpClient.AspNet.Tests.Unit.Validators;

public class CircuitBreakerSettingsValidatorTests
{
    private readonly CircuitBreakerSettingsValidator _validator = new();

    [Theory]
    [InlineData(0.1)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void GivenValidFailureThreshold_ShouldPass(double failureThreshold)
    {
        // Arrange
        var settings = new CircuitBreakerSettings { FailureThreshold = failureThreshold };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.1)]
    public void GivenFailureThresholdLessThanOrEqualToZero_ShouldFail(double failureThreshold)
    {
        // Arrange
        var settings = new CircuitBreakerSettings { FailureThreshold = failureThreshold };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CircuitBreakerSettings.FailureThreshold));
    }

    [Fact]
    public void GivenFailureThresholdGreaterThanOne_ShouldFail()
    {
        // Arrange
        var settings = new CircuitBreakerSettings { FailureThreshold = 1.1 };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CircuitBreakerSettings.FailureThreshold));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void GivenValidMinimumThroughput_ShouldPass(int minimumThroughput)
    {
        // Arrange
        var settings = new CircuitBreakerSettings { MinimumThroughput = minimumThroughput };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GivenMinimumThroughputLessThanOrEqualToZero_ShouldFail(int minimumThroughput)
    {
        // Arrange
        var settings = new CircuitBreakerSettings { MinimumThroughput = minimumThroughput };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CircuitBreakerSettings.MinimumThroughput));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10000)]
    [InlineData(30000)]
    public void GivenValidSampleDurationMilliseconds_ShouldPass(int sampleDurationMilliseconds)
    {
        // Arrange
        var settings = new CircuitBreakerSettings { SampleDurationMilliseconds = sampleDurationMilliseconds };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GivenSampleDurationMillisecondsLessThanOrEqualToZero_ShouldFail(int sampleDurationMilliseconds)
    {
        // Arrange
        var settings = new CircuitBreakerSettings { SampleDurationMilliseconds = sampleDurationMilliseconds };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CircuitBreakerSettings.SampleDurationMilliseconds));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5000)]
    [InlineData(60000)]
    public void GivenValidDurationOfBreakMilliseconds_ShouldPass(int durationOfBreakMilliseconds)
    {
        // Arrange
        var settings = new CircuitBreakerSettings { DurationOfBreakMilliseconds = durationOfBreakMilliseconds };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GivenDurationOfBreakMillisecondsLessThanOrEqualToZero_ShouldFail(int durationOfBreakMilliseconds)
    {
        // Arrange
        var settings = new CircuitBreakerSettings { DurationOfBreakMilliseconds = durationOfBreakMilliseconds };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CircuitBreakerSettings.DurationOfBreakMilliseconds));
    }
}