using System.Reflection;
using System.Text;

#pragma warning disable RS1035

namespace Lokad.Cli.Generator.Tests;

public static class RunInProcess
{
    private sealed record FailedTest(string DisplayName, Exception Exception);

    public static void RunTests(Assembly assembly)
    {
        var failedTests = new List<FailedTest>();
        var totalTests = 0;
        var skippedTests = 0;

        foreach (var type in assembly.GetTypes())
        {
            if (!type.IsClass || !type.IsPublic)
                continue;

            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            foreach (var method in methods)
            {
                var factAttributes = method.GetCustomAttributes<FactAttribute>(inherit: true).ToArray();
                if (factAttributes.Length == 0)
                    continue;

                totalTests++;
                if (factAttributes.Any(attribute => attribute.Skip is not null))
                {
                    skippedTests++;
                    continue;
                }

                object? instance = null;
                try
                {
                    instance = Activator.CreateInstance(type);
                    method.Invoke(instance, Array.Empty<object>());
                }
                catch (Exception ex)
                {
                    failedTests.Add(new FailedTest($"{type.FullName}.{method.Name}", UnwrapInvokeException(ex)));
                }
                finally
                {
                    if (instance is IDisposable disposable)
                        disposable.Dispose();
                }
            }
        }

        if (failedTests.Count > 0)
        {
            foreach (var failure in failedTests)
            {
                Console.WriteLine($"FAILED: {failure.DisplayName}");
                Console.WriteLine(FormatException(failure.Exception));
            }
        }

        Console.WriteLine($"Summary: {totalTests} tests, {failedTests.Count} failed, {skippedTests} skipped");
        Environment.Exit(failedTests.Count == 0 ? 0 : -1);
    }

    private static Exception UnwrapInvokeException(Exception exception) =>
        exception is TargetInvocationException { InnerException: var inner and not null }
            ? inner
            : exception;

    private static string FormatException(Exception exception)
    {
        var builder = new StringBuilder();
        var current = exception;
        var depth = 0;

        while (true)
        {
            var prefix = depth == 0 ? "Exception" : $"InnerException[{depth}]";
            builder.AppendLine($"{prefix}: {current.GetType().FullName}: {current.Message}");
            builder.AppendLine(current.StackTrace ?? "(no stack trace)");

            if (current.InnerException == null)
                break;

            builder.AppendLine();
            current = current.InnerException;
            depth++;
        }

        return builder.ToString();
    }
}
