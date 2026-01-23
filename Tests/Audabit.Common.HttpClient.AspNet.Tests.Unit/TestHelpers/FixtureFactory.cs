namespace Audabit.Common.HttpClient.AspNet.Tests.Unit.TestHelpers;

/// <summary>
/// Factory for creating configured AutoFixture instances for unit tests.
/// </summary>
public static class FixtureFactory
{
    /// <summary>
    /// Creates a new AutoFixture instance configured for testing.
    /// </summary>
    /// <returns>A configured <see cref="Fixture"/> instance.</returns>
    public static Fixture Create()
    {
        var fixture = new Fixture();

        return fixture;
    }
}