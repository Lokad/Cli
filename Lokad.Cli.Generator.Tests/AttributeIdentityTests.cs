using Microsoft.CodeAnalysis;

namespace Lokad.Cli.Generator.Tests;

public sealed class AttributeIdentityTests
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    public void IgnoresForeignAttributeNamesInEitherDeclarationOrder(bool foreignFirst, bool foreignGlobal)
    {
        const string foreignSource = """
            namespace Lokad.Cli;
            [System.AttributeUsage(System.AttributeTargets.Class)]
            public sealed class ArgumentsAttribute : System.Attribute
            {
                public ArgumentsAttribute(string documentation) { }
            }
            [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Parameter)]
            public sealed class ArgumentAttribute : System.Attribute
            {
                public ArgumentAttribute(string documentation) { }
            }
            [System.AttributeUsage(System.AttributeTargets.Interface, AllowMultiple = true)]
            public sealed class ChoiceAttribute : System.Attribute
            {
                public ChoiceAttribute(System.Type type) { }
            }
            """;
        var reference = GeneratorTestFixture.CompileReference("ForeignAnnotations", foreignSource)
            .WithAliases(foreignGlobal ? new[] { "global", "foreign" } : new[] { "foreign" });

        const string foreign = """
            [foreign::Lokad.Cli.Arguments("Foreign.")] public class ForeignMarked { }
            [foreign::Lokad.Cli.Choice(typeof(ForeignMarked))] public interface ForeignChoice { }
            """;
        const string actual = """
            [runtime::Lokad.Cli.Choice(typeof(Actual))]
            public partial interface IActual { int Value { get; } }
            [runtime::Lokad.Cli.Arguments("Actual.")]
            public partial record Actual([property: runtime::Lokad.Cli.Argument("Value.", Long = "value")] int Value) : IActual
            {
                [foreign::Lokad.Cli.Argument("Ignored.")] public int Ignored => 0;
            }
            """;
        const string usage = """
            public static class Usage
            {
                public static int Run() => Parser.Parse<IActual>(new[] { "--value=7" }).Value;
            }
            """;
        var source = "extern alias runtime; extern alias foreign; using Lokad.Cli; namespace Example; " +
            (foreignFirst ? foreign + actual : actual + foreign) + usage;
        var runtime = MetadataReference.CreateFromFile(typeof(ArgumentsAttribute).Assembly.Location).WithAliases(new[] { "runtime" });
        GeneratorTestFixture.AssertCompiles(source, new LokadCliGenerator(), new[] { reference, runtime });
    }
}
