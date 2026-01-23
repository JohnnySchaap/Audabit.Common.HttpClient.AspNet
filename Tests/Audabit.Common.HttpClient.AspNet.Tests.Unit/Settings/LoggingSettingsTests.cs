using Audabit.Common.HttpClient.AspNet.Settings;

namespace Audabit.Common.HttpClient.AspNet.Tests.Unit.Settings;

public class LoggingSettingsTests
{
    [Fact]
    public void DefaultSettings_ShouldHaveAllPropertiesFalse()
    {
        // Act
        var settings = new LoggingSettings();

        // Assert
        settings.LogRequest.ShouldBeFalse();
        settings.LogRequestHeaders.ShouldBeFalse();
        settings.LogRequestBody.ShouldBeFalse();
        settings.LogResponse.ShouldBeFalse();
        settings.LogResponseHeaders.ShouldBeFalse();
        settings.LogResponseBody.ShouldBeFalse();
    }

    [Fact]
    public void Init_ShouldAllowSettingAllProperties()
    {
        // Act
        var settings = new LoggingSettings
        {
            LogRequest = true,
            LogRequestHeaders = true,
            LogRequestBody = true,
            LogResponse = true,
            LogResponseHeaders = true,
            LogResponseBody = true
        };

        // Assert
        settings.LogRequest.ShouldBeTrue();
        settings.LogRequestHeaders.ShouldBeTrue();
        settings.LogRequestBody.ShouldBeTrue();
        settings.LogResponse.ShouldBeTrue();
        settings.LogResponseHeaders.ShouldBeTrue();
        settings.LogResponseBody.ShouldBeTrue();
    }

    [Theory]
    [InlineData(true, true, true, true, true, true)]
    [InlineData(false, false, false, false, false, false)]
    [InlineData(true, false, true, false, true, false)]
    public void Init_WithVariousCombinations_ShouldSetPropertiesCorrectly(
        bool logRequest,
        bool logRequestHeaders,
        bool logRequestBody,
        bool logResponse,
        bool logResponseHeaders,
        bool logResponseBody)
    {
        // Act
        var settings = new LoggingSettings
        {
            LogRequest = logRequest,
            LogRequestHeaders = logRequestHeaders,
            LogRequestBody = logRequestBody,
            LogResponse = logResponse,
            LogResponseHeaders = logResponseHeaders,
            LogResponseBody = logResponseBody
        };

        // Assert
        settings.LogRequest.ShouldBe(logRequest);
        settings.LogRequestHeaders.ShouldBe(logRequestHeaders);
        settings.LogRequestBody.ShouldBe(logRequestBody);
        settings.LogResponse.ShouldBe(logResponse);
        settings.LogResponseHeaders.ShouldBe(logResponseHeaders);
        settings.LogResponseBody.ShouldBe(logResponseBody);
    }
}