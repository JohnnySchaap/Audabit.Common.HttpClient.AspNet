using Audabit.Common.HttpClient.AspNet.Settings;
using Audabit.Common.HttpClient.AspNet.Validators;

namespace Audabit.Common.HttpClient.AspNet.Tests.Unit.Validators;

public class TimeoutSettingsValidatorTests
{
    private readonly TimeoutSettingsValidator _validator = new();

    [Theory]
    [InlineData(1)]
    [InlineData(30)]
    [InlineData(300)]
    public void GivenValidTimeoutSeconds_ShouldPass(int timeoutSeconds)
    {
        // Arrange
        var settings = new TimeoutSettings { TimeoutSeconds = timeoutSeconds };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GivenTimeoutSecondsLessThanOrEqualToZero_ShouldFail(int timeoutSeconds)
    {
        // Arrange
        var settings = new TimeoutSettings { TimeoutSeconds = timeoutSeconds };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(TimeoutSettings.TimeoutSeconds));
    }

    [Fact]
    public void GivenTimeoutSecondsGreaterThan300_ShouldFail()
    {
        // Arrange
        var settings = new TimeoutSettings { TimeoutSeconds = 301 };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(TimeoutSettings.TimeoutSeconds));
    }
}