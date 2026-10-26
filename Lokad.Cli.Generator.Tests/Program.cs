namespace Lokad.Cli.Generator.Tests;

public static class Program
{
    public static int Main(string[] args)
    {
        if (args is ["--reindex"])
        {
            GeneratorTestFixture.Reindex();
            return 0;
        }

        if (args is ["--refresh-all"])
        {
            GeneratorTestFixture.RefreshAll();
            return 0;
        }

        if (args is ["--refresh", var test])
        {
            GeneratorTestFixture.Refresh(test);
            return 0;
        }

        Console.Error.WriteLine("Use dotnet test to run tests, or --reindex, --refresh-all, --refresh <fixture> to maintain snapshots.");
        return 1;
    }
}
