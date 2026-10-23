using System;
using System.Collections.Generic;
using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class OptionalValueCommandTests
{
    [Theory]
    [InlineData(false, false, null)]
    [InlineData(true, false, null, "--format")]
    [InlineData(true, true, "json", "--format=json")]
    [InlineData(true, true, "", "--format=")]
    [InlineData(true, true, "xml", "--style=xml")]
    [InlineData(true, false, null, "-F")]
    [InlineData(true, true, "json", "-Fjson")]
    [InlineData(true, true, "", "-F=")]
    [InlineData(true, true, "json", "-Ujson")]
    [InlineData(true, false, null, "--format=json", "--style")]
    [InlineData(true, true, "json", "--format", "--style=json")]
    [InlineData(true, false, null, "-Fjson", "-U")]
    public void RetainsPresenceAndTheLastScalarOccurrence(bool present, bool hasValue, string? expected, params string[] arguments)
    {
        var parsed = Parser.Parse<OptionalValueArgs>(arguments);
        Assert.Equal(present, parsed.Format.IsPresent);
        Assert.Equal(hasValue, parsed.Format.HasValue);
        Assert.Equal(hasValue, parsed.Format.TryGetValue(out var value));
        Assert.Equal(expected, value);
    }

    [Fact]
    public void LeavesFollowingOperandsAndOptionsUnconsumed()
    {
        var parsed = Parser.Parse<OptionalValueArgs>(new[] { "--format", "input.txt", "--count=2", "--help" });
        Assert.True(parsed.Format.IsPresent);
        Assert.False(parsed.Format.HasValue);
        Assert.Equal("input.txt", parsed.Input);
        Assert.Equal(2, parsed.Count.Value);
        Assert.True(parsed.Help);

        var numeric = Parser.Parse<OptionalValueArgs>(new[] { "-F", "-5" });
        Assert.False(numeric.Format.HasValue);
        Assert.Equal(5, numeric.Limit);
    }

    [Fact]
    public void TreatsTheRestOfAShortArgumentAsValueText()
    {
        var parsed = Parser.Parse<OptionalValueArgs>(new[] { "-vFjson" });
        Assert.True(parsed.Verbose);
        Assert.Equal("json", parsed.Format.Value);
        Assert.False(parsed.J || parsed.S || parsed.O || parsed.N);
    }

    [Fact]
    public void PreservesTerminatorAndHelpLookingDataBoundaries()
    {
        var tokens = Parser.Tokenize<OptionalValueArgs>(new[] { "--format=--help" });
        Assert.False(tokens.HasLongOption("help"));
        var attached = Parser.ParseTokenized<OptionalValueArgs>(tokens);
        Assert.Equal("--help", attached.Format.Value);
        Assert.Null(tokens.Peek());

        var literal = Parser.Parse<OptionalValueArgs>(new[] { "--format", "--", "--help" });
        Assert.True(literal.Format.IsPresent);
        Assert.False(literal.Format.HasValue);
        Assert.Equal("--help", literal.Input);
        Assert.False(literal.Help);
    }

    [Fact]
    public void ConvertsTypedValuesThroughTheExistingHelpers()
    {
        var parsed = Parser.Parse<OptionalValueArgs>(new[] { "--count", "--mode=Monday", "--flag-value=false", "--label=alpha" });
        Assert.True(parsed.Count.IsPresent);
        Assert.False(parsed.Count.HasValue);
        Assert.Equal(DayOfWeek.Monday, parsed.Mode.Value);
        Assert.Equal(false, parsed.FlagValue.Value);
        Assert.Equal(new GenericLabelValue<string?>("alpha"), parsed.Label.Value);
    }

    [Theory]
    [InlineData("invalid value `bad` for `--count`", "--count=bad")]
    [InlineData("invalid value `` for `--count`", "--count=")]
    [InlineData("invalid value `bad` for `--flag-value`", "--flag-value=bad")]
    [InlineData("invalid value `bad` for `--label`", "--label=bad")]
    [InlineData("option `--required` expects a value", "--required")]
    public void PreservesConversionErrorsAndOrdinaryNullableOptionRules(string expected, params string[] arguments) =>
        Assert.Equal(expected, Assert.Throws<ParseException>(() => Parser.Parse<OptionalValueArgs>(arguments)).Message);

    [Fact]
    public void PreservesCustomParserExceptions()
    {
        Assert.Equal("Custom reference TryParse failure.", Assert.Throws<FormatException>(() =>
            Parser.Parse<OptionalValueArgs>(new[] { "--label=throw" })).Message);
    }

    [Fact]
    public void AccumulatesEveryCollectionOccurrenceAcrossContainers()
    {
        var arguments = new[] { "--count", "-n1", "--count", "--count=2" };
        var expected = new[] { OptionalValue<int?>.Bare, new OptionalValue<int?>(1), OptionalValue<int?>.Bare, new OptionalValue<int?>(2) };
        Assert.Equal(expected, Parser.Parse<OptionalArrayArgs>(arguments).Values);
        Assert.Equal(expected, Parser.Parse<OptionalListArgs>(arguments).Values);
        Assert.Equal(expected, Parser.Parse<OptionalMemoryArgs>(arguments).Values.ToArray());
        Assert.Empty(Parser.Parse<OptionalArrayArgs>(Array.Empty<string>()).Values);
        Assert.Empty(Parser.Parse<OptionalListArgs>(Array.Empty<string>()).Values);
        Assert.True(Parser.Parse<OptionalMemoryArgs>(Array.Empty<string>()).Values.IsEmpty);

        var strings = Parser.Parse<OptionalValueArgs>(new[] { "--tag", "--tag=", "--tag=text" }).Tags;
        Assert.Equal(new[] { OptionalValue<string>.Bare, new OptionalValue<string>(""), new OptionalValue<string>("text") }, strings);
    }

    [Fact]
    public void SharesChoiceAndTokenizedParsingWithoutConsumingDiscriminators()
    {
        var tokens = Parser.Tokenize<IOptionalValueCommand>(new[] { "format", "--format=--inspect" });
        Assert.False(tokens.HasLongOption("inspect"));
        var parsed = Assert.IsType<OptionalValueArgs>(Parser.ParseTokenized<IOptionalValueCommand>(tokens));
        Assert.Equal("--inspect", parsed.Format.Value);
        Assert.Null(tokens.Peek());

        var inspected = Assert.IsType<InspectOptionalArgs>(Parser.Parse<IOptionalValueCommand>(new[] { "--format", "--inspect" }));
        Assert.True(inspected.Format.IsPresent);
        Assert.False(inspected.Format.HasValue);
    }

    [Fact]
    public void DocumentsTheOptionalAttachedValueForEveryAlias()
    {
        Assert.Contains("--format[=style], --style[=style], -F[style], -U[style]: Format.", Parser.Document<OptionalValueArgs>("app"));
    }

    [Fact]
    public void ConvertsOnlyExplicitValuesAndOnlyOnce()
    {
        // Only this test uses the value type and owns its conversion counter.
        CountedOptionalValue.Calls = 0;
        var parsed = Parser.Parse<CountedOptionalArgs>(new[] { "--value", "--value=7", "--value" });
        Assert.True(parsed.Value.IsPresent);
        Assert.False(parsed.Value.HasValue);
        Assert.Equal(1, CountedOptionalValue.Calls);
    }
}

[Arguments("Optional values.", Prefix = "format", ShortOptionSyntax = ShortOptionSyntax.AttachedValues)]
public partial record OptionalValueArgs(
    [property: Argument("Format.", LongNames = new[] { "format", "style" }, ShortNames = new[] { 'F', 'U' }, Name = "style")]
    OptionalValue<string> Format) : IOptionalValueCommand
{
    [Argument("Count.", Long = "count", Name = "number")] public OptionalValue<int> Count;
    [Argument("Mode.", Long = "mode")] public OptionalValue<DayOfWeek?> Mode { get; init; }
    [Argument("Flag value.", Long = "flag-value")] public OptionalValue<bool?> FlagValue { get; init; }
    [Argument("Label.", Long = "label")] public OptionalValue<GenericLabelValue<string?>?> Label { get; init; }
    [Argument("Tags.", Long = "tag")] public IReadOnlyList<OptionalValue<string>> Tags { get; init; } = Array.Empty<OptionalValue<string>>();
    [Argument("Input.", Name = "input")] public string? Input { get; init; }
    [Argument("Required operand.", Long = "required")] public int? Required { get; init; }
    [Argument("Help.", Long = "help")] public bool Help { get; init; }
    [Argument("Verbose.", Short = 'v')] public bool Verbose { get; init; }
    [Argument("J.", Short = 'j')] public bool J { get; init; }
    [Argument("S.", Short = 's')] public bool S { get; init; }
    [Argument("O.", Short = 'o')] public bool O { get; init; }
    [Argument("N.", Short = 'n')] public bool N { get; init; }
    [Argument("Numeric short.", ShortNumeric = true)] public int? Limit { get; init; }
}

[Choice(typeof(OptionalValueArgs))]
[Choice(typeof(InspectOptionalArgs))]
public partial interface IOptionalValueCommand { }

[Arguments("Inspect.", Long = "inspect")]
public partial record InspectOptionalArgs : IOptionalValueCommand
{
    [Argument("Format.", Long = "format")] public OptionalValue<string> Format { get; init; }
}

[Arguments("Optional array.", ShortOptionSyntax = ShortOptionSyntax.AttachedValues)]
public partial record OptionalArrayArgs(
    [property: Argument("Values.", Long = "count", Short = 'n')] OptionalValue<int?>[] Values);

[Arguments("Optional list.", ShortOptionSyntax = ShortOptionSyntax.AttachedValues)]
public partial record OptionalListArgs
{
    [Argument("Values.", Long = "count", Short = 'n')] public IReadOnlyList<OptionalValue<int?>> Values { get; init; } = Array.Empty<OptionalValue<int?>>();
}

[Arguments("Optional memory.", ShortOptionSyntax = ShortOptionSyntax.AttachedValues)]
public partial record OptionalMemoryArgs
{
    [Argument("Values.", Long = "count", Short = 'n')] public ReadOnlyMemory<OptionalValue<int?>> Values;
}

[Arguments("Counted optional values.")]
public partial record CountedOptionalArgs(
    [property: Argument("Value.", Long = "value")] OptionalValue<CountedOptionalValue> Value);

public readonly record struct CountedOptionalValue(UnsignedId Value) : IParsable<CountedOptionalValue>
{
    public static int Calls { get; set; }
    public static CountedOptionalValue Parse(string value, IFormatProvider? provider) => throw new InvalidOperationException("Parse must not be called.");
    public static bool TryParse(string? value, IFormatProvider? provider, out CountedOptionalValue result)
    {
        Calls++;
        var success = UnsignedId.TryParse(value, provider, out var parsed);
        result = new CountedOptionalValue(parsed);
        return success;
    }
}
