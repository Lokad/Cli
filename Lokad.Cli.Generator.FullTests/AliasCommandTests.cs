using System;
using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class AliasCommandTests
{
    [Theory]
    [InlineData("-r")]
    [InlineData("-R")]
    [InlineData("--recursive")]
    [InlineData("--recurse")]
    [InlineData("-recurse")]
    [InlineData("-rR", "--recurse")]
    public void BindsRecursiveSpellingsToOneFlag(params string[] arguments)
    {
        Assert.True(Parser.Parse<AliasArgs>(arguments).Recursive);
    }

    [Theory]
    [InlineData("-qQ")]
    [InlineData("--silent", "-q")]
    [InlineData("--quiet", "--silent")]
    public void AppliesStrictRepetitionAcrossAliases(params string[] arguments)
    {
        var error = Assert.Throws<ParseException>(() => Parser.Parse<AliasArgs>(arguments));

        Assert.Equal("duplicate option `--quiet`", error.Message);
    }

    [Fact]
    public void PreservesNullableFlagAbsence()
    {
        Assert.Null(Parser.Parse<AliasArgs>(Array.Empty<string>()).Quiet);
        Assert.True(Parser.Parse<AliasArgs>(new[] { "-Q" }).Quiet);
    }

    [Theory]
    [InlineData("--number=7")]
    [InlineData("--number", "7")]
    [InlineData("-N7")]
    [InlineData("-N=7")]
    [InlineData("-N", "7")]
    public void SatisfiesRequiredConstructorArgumentsThroughAliases(params string[] arguments)
    {
        Assert.Equal(7, Parser.Parse<TokenizationConstructorArgs>(arguments).Count);
    }

    [Theory]
    [InlineData("missing option `--count`")]
    [InlineData("option `--count` expects a value", "--number")]
    [InlineData("invalid value `bad` for `--count`", "-N", "bad")]
    public void UsesPrimaryNamesForBindingErrors(string expectedMessage, params string[] arguments)
    {
        var error = Assert.Throws<ParseException>(() => Parser.Parse<TokenizationConstructorArgs>(arguments));

        Assert.Equal(expectedMessage, error.Message);
    }

    [Fact]
    public void PreservesScalarOverwriteAcrossAliases()
    {
        var parsed = Parser.Parse<AliasArgs>(new[] { "--output=first", "-O", "second", "--out=third" });

        Assert.Equal("third", parsed.Output);
    }

    [Fact]
    public void AccumulatesValuesAcrossAliases()
    {
        var parsed = Parser.Parse<AliasArgs>(new[] { "--tag", "one", "-T=two", "--label=three" });

        Assert.Equal(new[] { "one", "two", "three" }, parsed.Tags);
    }

    [Fact]
    public void PreservesLiteralValuesAndTerminatedOperands()
    {
        var parsed = Parser.Parse<AliasArgs>(new[] { "--out=--recurse", "--", "-R", "--label" });

        Assert.Equal("--recurse", parsed.Output);
        Assert.False(parsed.Recursive);
        Assert.Empty(parsed.Tags);
        Assert.Equal(new[] { "-R", "--label" }, parsed.Operands);
    }

    [Fact]
    public void RetainsOptionValueBoundaries()
    {
        var error = Assert.Throws<ParseException>(() => Parser.Parse<AliasArgs>(new[] { "--out", "-R" }));

        Assert.Equal("option `--output` expects a value", error.Message);
    }

    [Theory]
    [InlineData("-10")]
    [InlineData("--depth=10")]
    [InlineData("-D10")]
    public void RetainsNumericShortFormsWithNamedAliases(string argument)
    {
        Assert.Equal(10, Parser.Parse<AliasArgs>(new[] { argument }).Depth);
    }

    [Fact]
    public void UsesPrimaryAliasForInvalidNumericShortValues()
    {
        var error = Assert.Throws<ParseException>(() => Parser.Parse<AliasArgs>(new[] { "-999999999999" }));

        Assert.Equal("invalid value `999999999999` for `--depth`", error.Message);
    }

    [Fact]
    public void SharesAliasesAcrossChoiceBranchesWithoutSharingBindingState()
    {
        var tokens = Parser.Tokenize<IAliasCommand>(new[] { "first", "-R", "--recurse" });
        Assert.True(tokens.HasShortOption('R'));
        Assert.True(tokens.HasLongOption("recurse"));
        Assert.True(Assert.IsType<AliasArgs>(Parser.ParseTokenized<IAliasCommand>(tokens)).Recursive);

        Assert.True(Assert.IsType<OtherAliasArgs>(Parser.Parse<IAliasCommand>(new[] { "second", "--recurse" })).Walk);
        var error = Assert.Throws<ParseException>(() => Parser.Parse<IAliasCommand>(new[] { "second", "-RR" }));
        Assert.Equal("duplicate option `--walk`", error.Message);
    }

    [Fact]
    public void DocumentsEachSpellingAndDescriptionOnce()
    {
        var documentation = Parser.Document<AliasArgs>("cmd");

        Assert.Equal(string.Join(Environment.NewLine, new[]
        {
            "cmd first <operand...>: Alias command.",
            "  --recursive, --recurse, -r, -R: Recursive traversal.",
            "  --quiet, --silent, -q, -Q: Quiet output.",
            "  --output, --out, -o, -O: Output value.",
            "  --tag, --label, -t, -T: Tags.",
            "  --depth, -D, -d, -<number>: Numeric depth.",
            "  <operand...>: Operands."
        }), documentation);
    }
}

[Choice(typeof(AliasArgs))]
[Choice(typeof(OtherAliasArgs))]
public partial interface IAliasCommand
{
}

[Arguments("Alias command.", Prefix = "first")]
public partial record AliasArgs : IAliasCommand
{
    [Argument("Recursive traversal.", ShortNames = new[] { 'r', 'R', 'r', 'R' },
        LongNames = new[] { "recursive", "recurse", "recursive", "recurse" }, AllowRepeated = true)]
    public bool Recursive { get; init; }

    [Argument("Quiet output.", ShortNames = new[] { 'q', 'Q' }, LongNames = new[] { "quiet", "silent" })]
    public bool? Quiet;

    [Argument("Output value.", ShortNames = new[] { 'o', 'O' }, LongNames = new[] { "output", "out" })]
    public string? Output { get; init; }

    [Argument("Tags.", ShortNames = new[] { 't', 'T' }, LongNames = new[] { "tag", "label" })]
    public string[] Tags { get; init; } = [];

    [Argument("Numeric depth.", ShortNumeric = true, ShortNames = new[] { 'D', 'd' }, Long = "depth")]
    public int? Depth { get; init; }

    [Argument("Operands.", Name = "operand")]
    public string[] Operands { get; init; } = [];
}

[Arguments("Other alias command.", Prefix = "second")]
public partial record OtherAliasArgs : IAliasCommand
{
    [Argument("Walk files.", Short = 'R', LongNames = new[] { "walk", "recurse" })]
    public bool Walk { get; init; }
}
