using System;
using System.Collections.Generic;
using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class PositionalCollectionCommandTests
{
    [Theory]
    [InlineData(new[] { 10, 11 }, "10", "11", "label", "12")]
    [InlineData(new int[0], "label", "12")]
    [InlineData(new[] { 10 }, "10", "label", "12")]
    [InlineData(new[] { 10, 11 }, "--verbose", "10", "11", "label", "12")]
    [InlineData(new[] { 10, 11 }, "10", "11", "--verbose", "label", "12")]
    [InlineData(new[] { 10, 11 }, "10", "11", "-v", "label", "12")]
    [InlineData(new[] { 10, 11 }, "10", "11", "-p999", "label", "12")]
    [InlineData(new[] { 10, 11 }, "10", "11", "-pv", "999", "label", "12")]
    [InlineData(new[] { 10, 11 }, "10", "11", "--pattern", "999", "label", "12")]
    [InlineData(new[] { 10, 11 }, "10", "11", "--pattern=999", "label", "12")]
    [InlineData(new[] { 10, 11 }, "10", "11", "--verbose", "--pattern=999", "label", "12")]
    [InlineData(new int[0], "--pattern", "999", "label", "12")]
    [InlineData(new[] { 10, 11 }, "10", "11", "--", "label", "12")]
    public void BindsTheSameBoundariesAcrossContainers(int[] expected, params string[] arguments)
    {
        Check<ArrayBoundaryArgs>(arguments, expected);
        Check<ListBoundaryArgs>(arguments, expected);
        Check<MemoryBoundaryArgs>(arguments, expected);

        static void Check<T>(string[] values, int[] numbers) where T : ICliParsed<T>, IBoundaryResult
        {
            var parsed = Parser.Parse<T>(values);
            Assert.Equal(numbers, parsed.Values);
            Assert.Equal("label", parsed.Label);
            Assert.Equal(12, parsed.Count);
        }
    }

    [Theory]
    [InlineData("missing argument")]
    [InlineData("missing argument", "10", "11")]
    [InlineData("invalid value `bad` for `argument`", "label", "bad")]
    [InlineData("unexpected argument `extra`", "label", "12", "extra")]
    [InlineData("option `--pattern` expects a value", "10", "--pattern")]
    [InlineData("unknown option `--unknown`", "--unknown")]
    public void PreservesScalarAndOptionErrors(string expectedMessage, params string[] arguments)
    {
        Assert.Equal(expectedMessage, Assert.Throws<ParseException>(() => Parser.Parse<ArrayBoundaryArgs>(arguments)).Message);
        Assert.Equal(expectedMessage, Assert.Throws<ParseException>(() => Parser.Parse<ListBoundaryArgs>(arguments)).Message);
        Assert.Equal(expectedMessage, Assert.Throws<ParseException>(() => Parser.Parse<MemoryBoundaryArgs>(arguments)).Message);
    }

    [Fact]
    public void KeepsOptionOperandsOutOfPositionalCollections()
    {
        var parsed = Parser.Parse<ArrayBoundaryArgs>(new[] { "10", "--pattern", "999", "--verbose", "label", "12" });

        Assert.Equal(new[] { 10 }, parsed.Numbers);
        Assert.Equal("999", parsed.Pattern);
        Assert.True(parsed.Verbose);
        Assert.Equal("label", parsed.Label);
        Assert.Equal(12, parsed.Count);
    }

    [Fact]
    public void PreservesDefaultNonfinalCollectionConsumption()
    {
        var parsed = Parser.Parse<LegacyCollectionArgs>(new[] { "10", "11", "12" });

        Assert.Equal(new[] { 10 }, parsed.Numbers);
        Assert.Equal("11", parsed.Label);
        Assert.Equal(12, parsed.Count);
    }

    [Fact]
    public void SharesChoiceAndTokenizedBindingWithoutConsumingThePrefix()
    {
        var tokens = Parser.Tokenize<IBoundaryCommand>(new[] { "numbers", "10", "11", "label", "12" });
        var numbers = Assert.IsType<ArrayBoundaryArgs>(Parser.ParseTokenized<IBoundaryCommand>(tokens));
        Assert.Equal(new[] { 10, 11 }, numbers.Numbers);
        Assert.Null(tokens.Peek());

        var words = Assert.IsType<StringBoundaryArgs>(Parser.Parse<IBoundaryCommand>(new[] { "words", "one", "two", "--end", "label" }));
        Assert.Equal(new[] { "one", "two" }, words.Words);
        Assert.Equal("label", words.Label);
    }

    [Fact]
    public void DoesNotReserveTrailingValuesFromAnAlwaysConvertibleCollection()
    {
        var error = Assert.Throws<ParseException>(() => Parser.Parse<StringBoundaryArgs>(new[] { "one", "two", "label" }));

        Assert.Equal("missing argument", error.Message);
    }

    [Fact]
    public void LeavesFailedCustomCandidatesForTheFollowingScalar()
    {
        var parsed = Parser.Parse<CustomBoundaryArgs>(new[] { "1", "2", "label", "3", "4" });

        Assert.Equal(new[] { new UnsignedId(1), new UnsignedId(2) }, parsed.Ids.ToArray());
        Assert.Equal("label", parsed.Label);
        Assert.Equal(new[] { 3, 4 }, parsed.Tail);
    }

    [Fact]
    public void PropagatesCustomParserExceptionsAndInvalidFinalElements()
    {
        Assert.Equal("Custom TryParse failure.", Assert.Throws<FormatException>(() =>
            Parser.Parse<CustomBoundaryArgs>(new[] { "throw" })).Message);
        Assert.Equal("invalid value `bad` for `argument`", Assert.Throws<ParseException>(() =>
            Parser.Parse<CustomBoundaryArgs>(new[] { "1", "label", "2", "bad" })).Message);
    }

    [Fact]
    public void SupportsNullableReferenceElementsThroughTheSameTryParsePath()
    {
        var parsed = Parser.Parse<ReferenceBoundaryArgs>(new[] { "alpha", "beta", "bad" });

        Assert.Equal(new[] { new LabelValue("alpha"), new LabelValue("beta") }, parsed.Values);
        Assert.Equal("bad", parsed.Label);
    }

    [Fact]
    public void ClosesOnlyTheActiveCollectionAtAnOptionBoundary()
    {
        var parsed = Parser.Parse<MultipleBoundaryArgs>(new[] { "1", "first", "2", "3", "--end", "second" });

        Assert.Equal(new int?[] { 1 }, parsed.First);
        Assert.Equal("first", parsed.FirstLabel);
        Assert.Equal(new[] { 2, 3 }, parsed.Second);
        Assert.Equal("second", parsed.SecondLabel);
    }

    [Fact]
    public void TreatsNumericShortOptionsAsBoundariesAndTerminatedOptionsAsData()
    {
        var numeric = Parser.Parse<ArrayBoundaryArgs>(new[] { "10", "-5", "label", "12" });
        Assert.Equal(new[] { 10 }, numeric.Numbers);
        Assert.Equal(5, numeric.Limit);
        Assert.Equal("label", numeric.Label);

        var literal = Parser.Parse<ArrayBoundaryArgs>(new[] { "10", "--", "--verbose", "12" });
        Assert.Equal(new[] { 10 }, literal.Numbers);
        Assert.Equal("--verbose", literal.Label);
        Assert.False(literal.Verbose);
    }

    [Fact]
    public void ConvertsEachSuccessfulCandidateOnlyOnce()
    {
        // This value type is used only by this test, which owns the counter.
        CountingBoundaryValue.Calls = 0;
        var parsed = Parser.Parse<CountingBoundaryArgs>(new[] { "1", "2", "label" });

        Assert.Equal(new[] { new CountingBoundaryValue(new UnsignedId(1)), new CountingBoundaryValue(new UnsignedId(2)) }, parsed.Values);
        Assert.Equal("label", parsed.Label);
        Assert.Equal(3, CountingBoundaryValue.Calls);
    }
}

public interface IBoundaryResult
{
    IReadOnlyList<int> Values { get; }
    string Label { get; }
    int Count { get; }
}

[Choice(typeof(ArrayBoundaryArgs))]
[Choice(typeof(StringBoundaryArgs))]
public partial interface IBoundaryCommand { }

[Arguments("Array boundaries.", Prefix = "numbers")]
public partial record ArrayBoundaryArgs(
    [property: Argument("Numbers.", ConsumeUntilBoundary = true)] int[] Numbers) : IBoundaryResult, IBoundaryCommand
{
    public IReadOnlyList<int> Values => Numbers;
    [Argument("Label.")] public string Label { get; init; } = "";
    [Argument("Count.")] public int Count { get; init; }
    [Argument("Verbose.", Long = "verbose", Short = 'v')] public bool Verbose { get; init; }
    [Argument("Pattern.", Long = "pattern", Short = 'p')] public string? Pattern { get; init; }
    [Argument("Limit.", ShortNumeric = true)] public int? Limit { get; init; }
}

[Arguments("List boundaries.")]
public partial record ListBoundaryArgs : IBoundaryResult
{
    [Argument("Numbers.", ConsumeUntilBoundary = true)] public IReadOnlyList<int> Numbers { get; init; } = Array.Empty<int>();
    public IReadOnlyList<int> Values => Numbers;
    [Argument("Label.")] public string Label { get; init; } = "";
    [Argument("Count.")] public int Count { get; init; }
    [Argument("Verbose.", Long = "verbose", Short = 'v')] public bool Verbose { get; init; }
    [Argument("Pattern.", Long = "pattern", Short = 'p')] public string? Pattern { get; init; }
}

[Arguments("Memory boundaries.")]
public partial record MemoryBoundaryArgs : IBoundaryResult
{
    [Argument("Numbers.", ConsumeUntilBoundary = true)] public ReadOnlyMemory<int> Numbers;
    public IReadOnlyList<int> Values => Numbers.ToArray();
    [Argument("Label.")] public string Label { get; init; } = "";
    [Argument("Count.")] public int Count { get; init; }
    [Argument("Verbose.", Long = "verbose", Short = 'v')] public bool Verbose { get; init; }
    [Argument("Pattern.", Long = "pattern", Short = 'p')] public string? Pattern { get; init; }
}

[Arguments("Legacy collections.")]
public partial record LegacyCollectionArgs
{
    [Argument("Numbers.")] public int[] Numbers { get; init; } = [];
    [Argument("Label.")] public string Label { get; init; } = "";
    [Argument("Count.")] public int Count { get; init; }
}

[Arguments("String boundaries.", Prefix = "words")]
public partial record StringBoundaryArgs : IBoundaryCommand
{
    [Argument("Words.", ConsumeUntilBoundary = true)] public string[] Words { get; init; } = [];
    [Argument("Label.")] public string Label { get; init; } = "";
    [Argument("End.", Long = "end")] public bool End { get; init; }
}

[Arguments("Custom boundaries.")]
public partial record CustomBoundaryArgs
{
    [Argument("Identifiers.", ConsumeUntilBoundary = true)] public ReadOnlyMemory<UnsignedId> Ids { get; init; }
    [Argument("Label.")] public string Label { get; init; } = "";
    [Argument("Tail.")] public int[] Tail { get; init; } = [];
}

[Arguments("Reference boundaries.")]
public partial record ReferenceBoundaryArgs
{
    [Argument("Values.", ConsumeUntilBoundary = true)] public IReadOnlyList<LabelValue?> Values { get; init; } = Array.Empty<LabelValue?>();
    [Argument("Label.")] public string Label { get; init; } = "";
}

[Arguments("Multiple boundaries.")]
public partial record MultipleBoundaryArgs
{
    [Argument("First.", ConsumeUntilBoundary = true)] public int?[] First { get; init; } = [];
    [Argument("First label.")] public string FirstLabel { get; init; } = "";
    [Argument("Second.", ConsumeUntilBoundary = true)] public int[] Second { get; init; } = [];
    [Argument("Second label.")] public string SecondLabel { get; init; } = "";
    [Argument("End.", Long = "end")] public bool End { get; init; }
}

[Arguments("Counting conversions.")]
public partial record CountingBoundaryArgs
{
    [Argument("Values.", ConsumeUntilBoundary = true)] public CountingBoundaryValue[] Values { get; init; } = [];
    [Argument("Label.")] public string Label { get; init; } = "";
}

public readonly record struct CountingBoundaryValue(UnsignedId Value) : IParsable<CountingBoundaryValue>
{
    public static int Calls { get; set; }

    public static CountingBoundaryValue Parse(string value, IFormatProvider? provider) =>
        throw new InvalidOperationException("Parse must not be called.");

    public static bool TryParse(string? value, IFormatProvider? provider, out CountingBoundaryValue result)
    {
        Calls++;
        var success = UnsignedId.TryParse(value, provider, out var parsed);
        result = new CountingBoundaryValue(parsed);
        return success;
    }
}
