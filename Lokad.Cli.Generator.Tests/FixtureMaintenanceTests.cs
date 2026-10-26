namespace Lokad.Cli.Generator.Tests;

public sealed class FixtureMaintenanceTests
{
    [Fact]
    public void RejectsUnknownFixtureNames()
    {
        Assert.Equal(1, Program.Main(new[] { "--refresh", "__MissingFixtureForRefreshTest" }));
    }
}
