using System;
using System.Collections.Generic;
using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class CollectionCommandTests
{
    [Theory]
    [MemberData(nameof(GeneratorFullTests.StringArrayOptionCases), MemberType = typeof(GeneratorFullTests))]
    public void ParsesReadOnlyStringCollectionsLikeArrays(string[] expected, string[] arguments)
    {
        Assert.Equal(expected, Parser.Parse<ListCollectionArgs>(arguments).X);
        Assert.Equal(expected, Parser.Parse<MemoryCollectionArgs>(arguments).X.ToArray());
    }

    [Fact]
    public void AccumulatesNumericOptionsAcrossGroupsAndAliases()
    {
        var arguments = new[] { "-nn", "1", "2", "--num=3", "-N4" };
        var expected = new[] { 1, 2, 3, 4 };

        Assert.Equal(expected, Parser.Parse<ListCollectionArgs>(arguments).Numbers);
        Assert.Equal(expected, Parser.Parse<MemoryCollectionArgs>(arguments).Numbers.ToArray());
    }

    [Fact]
    public void BindsFinalCustomCollectionsThroughConstructors()
    {
        var arguments = new[] { "--xval=--help", "--", "3", "4" };
        var expected = new[] { new UnsignedId(3), new UnsignedId(4) };
        var list = Parser.Parse<ListCollectionArgs>(arguments);
        var memory = Parser.Parse<MemoryCollectionArgs>(arguments);

        Assert.Equal(expected, list.Ids);
        Assert.Equal(expected, memory.Ids.ToArray());
        Assert.Equal(new[] { "--help" }, list.X);
        Assert.Equal(new[] { "--help" }, memory.X.ToArray());
    }

    [Fact]
    public void ProducesUsableEmptyCollections()
    {
        var list = Parser.Parse<ListCollectionArgs>(Array.Empty<string>());
        var memory = Parser.Parse<MemoryCollectionArgs>(Array.Empty<string>());

        Assert.Empty(list.X);
        Assert.Empty(list.Numbers);
        Assert.Empty(list.Ids);
        Assert.Empty(list.Flags);
        Assert.Empty(list.OptionalNumbers);
        Assert.True(memory.X.IsEmpty);
        Assert.True(memory.Numbers.IsEmpty);
        Assert.True(memory.Ids.IsEmpty);
        Assert.True(memory.Flags.IsEmpty);
        Assert.True(memory.OptionalNumbers.IsEmpty);
    }

    [Theory]
    [InlineData("invalid value `bad` for `--number`", "--num=bad")]
    [InlineData("invalid value `bad` for `--number`", "-N", "bad")]
    [InlineData("option `--number` expects a value", "--number")]
    [InlineData("option `--number` expects a value", "--number", "--xval=value")]
    [InlineData("invalid value `bad` for `argument`", "1", "bad")]
    [InlineData("invalid value `4294967296` for `argument`", "4294967296")]
    [InlineData("invalid value `bad` for `--flag-value`", "--flag-value=bad")]
    public void RetainsElementErrorsAndMissingValueBoundaries(string expectedMessage, params string[] arguments)
    {
        var listError = Assert.Throws<ParseException>(() => Parser.Parse<ListCollectionArgs>(arguments));
        var memoryError = Assert.Throws<ParseException>(() => Parser.Parse<MemoryCollectionArgs>(arguments));

        Assert.Equal(expectedMessage, listError.Message);
        Assert.Equal(expectedMessage, memoryError.Message);
    }

    [Fact]
    public void PropagatesExceptionsFromCustomElementParsers()
    {
        Assert.Equal("Custom TryParse failure.", Assert.Throws<FormatException>(() =>
            Parser.Parse<ListCollectionArgs>(new[] { "throw" })).Message);
        Assert.Equal("Custom TryParse failure.", Assert.Throws<FormatException>(() =>
            Parser.Parse<MemoryCollectionArgs>(new[] { "throw" })).Message);
    }

    [Fact]
    public void ParsesBooleanAndNullableElementsAcrossContainers()
    {
        var arguments = new[] { "--flag-value=true", "--flag-value", "false", "--optional-number=7" };
        var list = Parser.Parse<ListCollectionArgs>(arguments);
        var memory = Parser.Parse<MemoryCollectionArgs>(arguments);
        var array = Parser.Parse<ArrayValueCollectionArgs>(arguments);
        var flags = new[] { true, false };
        var numbers = new int?[] { 7 };

        Assert.Equal(flags, list.Flags);
        Assert.Equal(flags, memory.Flags.ToArray());
        Assert.Equal(flags, array.Flags);
        Assert.Equal(numbers, list.OptionalNumbers);
        Assert.Equal(numbers, memory.OptionalNumbers.ToArray());
        Assert.Equal(numbers, array.OptionalNumbers);
    }

    [Fact]
    public void SharesTheGeneratedTokenizationPath()
    {
        var arguments = new[] { "--num=1", "-N2", "3", "4" };
        var list = Parser.ParseTokenized<ListCollectionArgs>(Parser.Tokenize<ListCollectionArgs>(arguments));
        var memory = Parser.ParseTokenized<MemoryCollectionArgs>(Parser.Tokenize<MemoryCollectionArgs>(arguments));

        Assert.Equal(new[] { 1, 2 }, list.Numbers);
        Assert.Equal(new[] { 1, 2 }, memory.Numbers.ToArray());
        Assert.Equal(new[] { new UnsignedId(3), new UnsignedId(4) }, list.Ids);
        Assert.Equal(list.Ids, memory.Ids.ToArray());
    }

    [Fact]
    public void DocumentsCollectionsWithTheSameLabelsAsArrays()
    {
        var expected = string.Join(Environment.NewLine, new[]
        {
            "cmd <id...>: Collection command.",
            "  <id...>: Identifiers.",
            "  --xval, -x: Adds an X value.",
            "  --number, --num, -n, -N: Numeric values.",
            "  --flag-value: Boolean values.",
            "  --optional-number: Nullable numeric values."
        });

        Assert.Equal(expected, Parser.Document<ListCollectionArgs>("cmd"));
        Assert.Equal(expected, Parser.Document<MemoryCollectionArgs>("cmd"));
    }
}

[Arguments("Collection command.")]
public partial record ListCollectionArgs(
    [property: Argument("Identifiers.", Name = "id")] IReadOnlyList<UnsignedId> Ids)
{
    [Argument("Adds an X value.", Short = 'x', Long = "xval")]
    public IReadOnlyList<string> X { get; init; } = Array.Empty<string>();

    [Argument("Numeric values.", ShortNames = new[] { 'n', 'N' }, LongNames = new[] { "number", "num" })]
    public IReadOnlyList<int> Numbers { get; init; } = Array.Empty<int>();

    [Argument("Boolean values.", Long = "flag-value")]
    public IReadOnlyList<bool> Flags { get; init; } = Array.Empty<bool>();

    [Argument("Nullable numeric values.", Long = "optional-number")]
    public IReadOnlyList<int?> OptionalNumbers { get; init; } = Array.Empty<int?>();
}

[Arguments("Collection command.")]
public partial record MemoryCollectionArgs(
    [Argument("Identifiers.", Name = "id")] ReadOnlyMemory<UnsignedId> Ids)
{
    [Argument("Adds an X value.", Short = 'x', Long = "xval")]
    public ReadOnlyMemory<string> X { get; init; }

    [Argument("Numeric values.", ShortNames = new[] { 'n', 'N' }, LongNames = new[] { "number", "num" })]
    public ReadOnlyMemory<int> Numbers;

    [Argument("Boolean values.", Long = "flag-value")]
    public ReadOnlyMemory<bool> Flags { get; init; }

    [Argument("Nullable numeric values.", Long = "optional-number")]
    public ReadOnlyMemory<int?> OptionalNumbers { get; init; }
}

[Arguments("Array values.")]
public partial record ArrayValueCollectionArgs
{
    [Argument("Boolean values.", Long = "flag-value")]
    public bool[] Flags { get; init; } = [];

    [Argument("Nullable numeric values.", Long = "optional-number")]
    public int?[] OptionalNumbers { get; init; } = [];
}
