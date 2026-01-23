using Audabit.Common.HttpClient.AspNet.Telemetry;

namespace Audabit.Common.HttpClient.AspNet.Tests.Unit.Telemetry;

public class HttpClientTimeoutEventTests
{
    [Fact]
    public void Constructor_ShouldCreateEventSuccessfully()
    {
        // Arrange
        const string clientName = "TestClient";
        const double timeoutSeconds = 30.5;

        // Act
        var @event = new HttpClientTimeoutEvent(clientName, timeoutSeconds);

        // Assert
        @event.ShouldNotBeNull();
    }

    [Theory]
    [InlineData("Client1", 10.0)]
    [InlineData("Client2", 60.5)]
    [InlineData("Client3", 120.25)]
    public void Constructor_WithVariousValues_ShouldCreateEventSuccessfully(string clientName, double timeoutSeconds)
    {
        // Act
        var @event = new HttpClientTimeoutEvent(clientName, timeoutSeconds);

        // Assert
        @event.ShouldNotBeNull();
    }
}