using System.Reflection;
using Lokad.Cli.Generator.Tests;

namespace Lokad.Cli.Tests;

public static class Program
{
    public static void Main()
    {
        RunInProcess.RunTests(Assembly.GetExecutingAssembly());
    }
}
