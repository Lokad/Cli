using System.Reflection;

namespace Lokad.Cli.Generator.Tests;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args is ["--reindex"])
        {
            GeneratorTestFixture.Reindex();
            return;
        }

        if (args is ["--refresh-all"])
        {
            GeneratorTestFixture.RefreshAll();
            return;
        }

        if (args is ["--refresh", var test])
        {
            GeneratorTestFixture.Refresh(test);
            return;
        }

        RunInProcess.RunTests(Assembly.GetExecutingAssembly());
    }
}
