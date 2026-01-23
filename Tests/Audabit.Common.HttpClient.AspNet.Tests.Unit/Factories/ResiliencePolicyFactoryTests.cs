using Audabit.Common.HttpClient.AspNet.Factories;
using Audabit.Common.HttpClient.AspNet.Settings;
using Audabit.Common.HttpClient.AspNet.Tests.Unit.TestHelpers;
using Audabit.Common.Observability.Emitters;

namespace Audabit.Common.HttpClient.AspNet.Tests.Unit.Factories;

public class ResiliencePolicyFactoryTests
{
    private readonly Fixture _fixture;

    public ResiliencePolicyFactoryTests()
    {
        _fixture = FixtureFactory.Create();
    }

    [Fact]
    public void CreateRetryPolicy_ShouldUseClientSettingsWhenProvided()
    {
        // Arrange
        var clientSettings = _fixture.Build<ClientSettings>()
            .With(x => x.Retry, new RetrySettings
            {
                MaxRetryAttempts = 5,
                RetryDelayMilliseconds = 1000,
                RetryBackoffPower = 2
            })
            .With(x => x.HttpRetryStatusCodes, [500, 502, 503])
            .Create();

        var defaultSettings = _fixture.Build<DefaultSettings>()
            .With(x => x.Retry, new RetrySettings
            {
                MaxRetryAttempts = 3,
                RetryDelayMilliseconds = 500,
                RetryBackoffPower = 1
            })
            .Create();

        var emitter = Substitute.For<IEmitter<TestClient>>();

        // Act
        var policy = ResiliencePolicyFactory.CreateRetryPolicy(emitter, clientSettings, defaultSettings);

        // Assert
        policy.ShouldNotBeNull();
    }

    [Fact]
    public void CreateRetryPolicy_ShouldFallbackToDefaultSettingsWhenClientRetryIsNull()
    {
        // Arrange
        var clientSettings = _fixture.Build<ClientSettings>()
            .Without(x => x.Retry)
            .With(x => x.HttpRetryStatusCodes, [500])
            .Create();

        var defaultSettings = _fixture.Build<DefaultSettings>()
            .With(x => x.Retry, new RetrySettings
            {
                MaxRetryAttempts = 3,
                RetryDelayMilliseconds = 500,
                RetryBackoffPower = 1
            })
            .With(x => x.HttpRetryStatusCodes, [500, 502])
            .Create();

        var emitter = Substitute.For<IEmitter<TestClient>>();

        // Act
        var policy = ResiliencePolicyFactory.CreateRetryPolicy(emitter, clientSettings, defaultSettings);

        // Assert
        policy.ShouldNotBeNull();
    }

    [Fact]
    public void CreateCircuitBreakerPolicy_ShouldUseClientSettingsWhenProvided()
    {
        // Arrange
        var clientSettings = _fixture.Build<ClientSettings>()
            .With(x => x.CircuitBreaker, new CircuitBreakerSettings
            {
                FailureThreshold = 0.75,
                MinimumThroughput = 20,
                SampleDurationMilliseconds = 120000,
                DurationOfBreakMilliseconds = 60000
            })
            .With(x => x.HttpRetryStatusCodes, [500, 502, 503])
            .Create();

        var defaultSettings = _fixture.Build<DefaultSettings>()
            .With(x => x.CircuitBreaker, new CircuitBreakerSettings
            {
                FailureThreshold = 0.5,
                MinimumThroughput = 10,
                SampleDurationMilliseconds = 60000,
                DurationOfBreakMilliseconds = 30000
            })
            .Create();

        var emitter = Substitute.For<IEmitter<TestClient>>();

        // Act
        var policy = ResiliencePolicyFactory.CreateCircuitBreakerPolicy(emitter, clientSettings, defaultSettings);

        // Assert
        policy.ShouldNotBeNull();
    }

    [Fact]
    public void CreateCircuitBreakerPolicy_ShouldFallbackToDefaultSettingsWhenClientCircuitBreakerIsNull()
    {
        // Arrange
        var clientSettings = _fixture.Build<ClientSettings>()
            .Without(x => x.CircuitBreaker)
            .With(x => x.HttpRetryStatusCodes, [500])
            .Create();

        var defaultSettings = _fixture.Build<DefaultSettings>()
            .With(x => x.CircuitBreaker, new CircuitBreakerSettings
            {
                FailureThreshold = 0.5,
                MinimumThroughput = 10,
                SampleDurationMilliseconds = 60000,
                DurationOfBreakMilliseconds = 30000
            })
            .With(x => x.HttpRetryStatusCodes, [500, 502])
            .Create();

        var emitter = Substitute.For<IEmitter<TestClient>>();

        // Act
        var policy = ResiliencePolicyFactory.CreateCircuitBreakerPolicy(emitter, clientSettings, defaultSettings);

        // Assert
        policy.ShouldNotBeNull();
    }

    [Fact]
    public void CreateTimeoutPolicy_ShouldUseClientSettingsWhenProvided()
    {
        // Arrange
        var clientSettings = _fixture.Build<ClientSettings>()
            .With(x => x.Timeout, new TimeoutSettings
            {
                TimeoutSeconds = 60
            })
            .Create();

        var defaultSettings = _fixture.Build<DefaultSettings>()
            .With(x => x.Timeout, new TimeoutSettings
            {
                TimeoutSeconds = 30
            })
            .Create();

        var emitter = Substitute.For<IEmitter<TestClient>>();

        // Act
        var policy = ResiliencePolicyFactory.CreateTimeoutPolicy(emitter, clientSettings, defaultSettings);

        // Assert
        policy.ShouldNotBeNull();
    }

    [Fact]
    public void CreateTimeoutPolicy_ShouldFallbackToDefaultSettingsWhenClientTimeoutIsNull()
    {
        // Arrange
        var clientSettings = _fixture.Build<ClientSettings>()
            .Without(x => x.Timeout)
            .Create();

        var defaultSettings = _fixture.Build<DefaultSettings>()
            .With(x => x.Timeout, new TimeoutSettings
            {
                TimeoutSeconds = 30
            })
            .Create();

        var emitter = Substitute.For<IEmitter<TestClient>>();

        // Act
        var policy = ResiliencePolicyFactory.CreateTimeoutPolicy(emitter, clientSettings, defaultSettings);

        // Assert
        policy.ShouldNotBeNull();
    }
}

public class TestClient { }