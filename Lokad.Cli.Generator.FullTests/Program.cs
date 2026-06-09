using System.Reflection;
using Lokad.Cli.Generator.Tests;

namespace Lokad.Cli.Generator.FullTests;

public static class Program
{
    public static void Main()
    {
        RunInProcess.RunTests(Assembly.GetExecutingAssembly());
    }
}
