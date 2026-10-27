using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Lokad.Cli.Generator.Tests;

public sealed class AttributeIdentityTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IgnoresForeignAttributeNamesInEitherDeclarationOrder(bool foreignFirst)
    {
        const string foreignSource = """
            namespace Lokad.Cli;
            [System.AttributeUsage(System.AttributeTargets.Class)]
            public sealed class ArgumentsAttribute : System.Attribute
            {
                public ArgumentsAttribute(string documentation) { }
            }
            """;
        var compilation = CSharpCompilation.Create("ForeignAnnotations",
            new[] { CSharpSyntaxTree.ParseText(foreignSource, new CSharpParseOptions(LanguageVersion.CSharp11)) },
            new[] { MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location) },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var stream = new MemoryStream();
        var emitted = compilation.Emit(stream);
        Assert.True(emitted.Success, string.Join("\n", emitted.Diagnostics));
        var reference = MetadataReference.CreateFromImage(stream.ToArray()).WithAliases(new[] { "foreign" });

        const string foreign = "[foreign::Lokad.Cli.Arguments(\"Foreign.\")] public class ForeignMarked { }";
        const string actual = """
            [Arguments("Actual.")]
            public partial record Actual([property: Argument("Value.", Long = "value")] int Value);
            """;
        const string usage = """
            public static class Usage
            {
                public static int Run() => Parser.Parse<Actual>(new[] { "--value=7" }).Value;
            }
            """;
        var source = "extern alias foreign; using Lokad.Cli; namespace Example; " +
            (foreignFirst ? foreign + actual : actual + foreign) + usage;
        GeneratorTestFixture.AssertCompiles(source, new LokadCliGenerator(), new[] { reference });
    }
}
