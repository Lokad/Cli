using Microsoft.CodeAnalysis;

namespace Lokad.Cli.Generator.Tests;

public sealed class AliasedValueTests
{
    [Theory]
    [InlineData(false, false, "values", false)]
    [InlineData(true, false, "values", false)]
    [InlineData(false, true, "values", false)]
    [InlineData(true, true, "values", false)]
    [InlineData(false, true, "class", false)]
    [InlineData(false, false, "values", true)]
    [InlineData(true, false, "values", true)]
    public void CompilesAliasedValuesInScalarsCollectionsAndGroups(bool globallyVisible, bool conflicting, string alias, bool hidesNamespace)
    {
        const string values = """
            #nullable enable
            namespace Values;
            public static class Catalog { public enum Mode { One, Two } }
            public readonly record struct Number<T>(int Value) : System.IParsable<Number<T>>
            {
                public static Number<T> Parse(string text, System.IFormatProvider? provider) => new(int.Parse(text, provider));
                public static bool TryParse(string? text, System.IFormatProvider? provider, out Number<T> result)
                {
                    var success = int.TryParse(text, provider, out var value);
                    result = new Number<T>(value);
                    return success;
                }
            }
            """;
        var reference = GeneratorTestFixture.CompileReference("Values", values)
            .WithAliases(globallyVisible ? new[] { "global", alias } : new[] { alias });
        var references = new List<MetadataReference> { reference };
        if (conflicting)
            references.Add(GeneratorTestFixture.CompileReference("OtherValues", values));

        const string source = """
            extern alias @ALIAS;
            #nullable enable
            using System;
            using System.Collections.Generic;
            using Lokad.Cli;
            namespace Example;
            [Arguments("Group.")]
            public partial record Group(
                [property: Argument("Nullable.", Long = "nullable")] @ALIAS::Values.Number<@ALIAS::Values.Catalog.Mode>? Nullable,
                [property: Argument("Many.", Long = "many")] IReadOnlyList<@ALIAS::Values.Number<@ALIAS::Values.Catalog.Mode?[]?>?> Many);
            [Arguments("Aliased values.")]
            public partial record Args(
                [property: Argument("Value.", Long = "value")] @ALIAS::Values.Number<@ALIAS::Values.Catalog.Mode[]> Value,
                [property: Argument("Mode.", Long = "mode")] @ALIAS::Values.Catalog.Mode Mode,
                [property: Argument("Modes.", Long = "modes")] ReadOnlyMemory<@ALIAS::Values.Catalog.Mode> Modes,
                [property: Argument("Format.", Long = "format")] OptionalValue<@ALIAS::Values.Number<int>> Format,
                [property: Argument("Group.")] Group Other);
            public static class Usage
            {
                public static int Run() => Parser.Parse<Args>(new[] { "--value=7", "--mode=One" }).Value.Value;
            }
            """;
        var consumer = source.Replace("ALIAS", alias);
        if (hidesNamespace)
            consumer = consumer.Replace("namespace Example;", "namespace Example {") + "\n}\npublic class Values { }";
        GeneratorTestFixture.AssertCompiles(consumer, new LokadCliGenerator(), references);
    }
}
