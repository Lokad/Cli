using Microsoft.CodeAnalysis;
using Xunit.Sdk;

namespace Lokad.Cli.Generator.Tests;

public sealed class CompilationValidationTests
{
    [Fact]
    public void RejectsSemanticCompilerErrors()
    {
        var error = Assert.ThrowsAny<XunitException>(() => GeneratorTestFixture.AssertCompiles(
            "namespace Example; public class Broken { public MissingType Value { get; set; } }"));
        Assert.Contains("CS0246", error.Message);
    }

    [Fact]
    public void RejectsGeneratorFailures()
    {
        var error = Assert.ThrowsAny<XunitException>(() => GeneratorTestFixture.AssertCompiles("", new BrokenGenerator()));
        Assert.Contains("Generator failed", error.Message);
    }

    private sealed class BrokenGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context) =>
            throw new InvalidOperationException("A failed generator must fail the fixture.");
    }
}
