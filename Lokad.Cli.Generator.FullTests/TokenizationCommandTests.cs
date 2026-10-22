using System;
using System.Collections.Generic;
using System.Linq;
using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class TokenizationCommandTests
{
    [Fact]
    public void TokenizesWithoutBindingValuesOrResolvingChoices()
    {
        Assert.Null(Parser.Tokenize<RangesArgs>(Array.Empty<string>()).Shift());

        var values = Parser.Tokenize<CustomValueArgs>(new[] { "--id=invalid" });
        Assert.Equal(new LongArgWithValue("id", "invalid"), values.Peek());
        Assert.Throws<ParseException>(() => Parser.ParseTokenized<CustomValueArgs>(values));

        var choices = Parser.Tokenize<ILongOptionChoiceArgs>(new[] { "--alpha", "--beta" });
        Assert.True(choices.HasLongOption("alpha"));
        Assert.True(choices.HasLongOption("beta"));
        Assert.Throws<ParseException>(() => Parser.ParseTokenized<ILongOptionChoiceArgs>(choices));
    }

    [Fact]
    public void ReturnsIndependentCursorsWithNestedChoiceMetadata()
    {
        var arguments = new[] { "sub", "B", "positional", "--first=named" };
        var first = Parser.Tokenize<INestedChoiceWithMembersArgs>(arguments);
        var second = Parser.Tokenize<INestedChoiceWithMembersArgs>(arguments);

        Assert.NotSame(first, second);
        var expected = Parser.Parse<INestedChoiceWithMembersArgs>(arguments);
        Assert.Equal(expected, Parser.ParseTokenized<INestedChoiceWithMembersArgs>(first));
        Assert.Equal(expected, Parser.ParseTokenized<INestedChoiceWithMembersArgs>(second));
    }

    [Fact]
    public void TokenizesConstructorArgumentsOnRecordStructs()
    {
        var tokens = TokenizationConstructorArgs.Tokenize(new[] { "--count=7" });

        Assert.Equal(new LongArgWithValue("count", "7"), tokens.Peek());
        Assert.Equal(7, Parser.ParseTokenized<TokenizationConstructorArgs>(tokens).Count);
    }

    [Fact]
    public void ParsesFromOwnStaticInitializer()
    {
        Assert.Equal(7, TokenizationConstructorArgs.Default.Count);
        Assert.Equal(9, Parser.Parse<TokenizationConstructorArgs>(new[] { "--count=9" }).Count);
    }

    [Fact]
    public void RetainsNumericShortOptions()
    {
        var tokens = Parser.Tokenize<ShortNumericArgs>(new[] { "-10" });

        Assert.Equal(new NumericShortArg("10"), tokens.Peek());
        Assert.Equal(10, Parser.ParseTokenized<ShortNumericArgs>(tokens).Value);
    }

    [Fact]
    public void AllowsTokenRewritingWithoutRetokenizingValues()
    {
        var tokens = Parser.Tokenize<FormattingArgs>(new[]
        {
            "-ccV", "--indent=2", "--from-file=--tab", "--", "--compact-output"
        });
        var flags = Assert.IsType<ShortArgSet>(tokens.Shift());
        Assert.Equal("ccV", flags.Options);

        // Only this known flag-only group is normalized; retain every subsequent token.
        var normalized = new List<ITokenizedArguments>
        {
            new ShortArgSet(new string(flags.Options.Distinct().ToArray()))
        };
        while (tokens.Shift() is { } token)
            normalized.Add(token);

        var parsed = Parser.ParseTokenized<FormattingArgs>(new TokenizedArguments(normalized));

        Assert.True(parsed.Compact);
        Assert.True(parsed.Version);
        Assert.False(parsed.Tab);
        Assert.Equal(2, parsed.Indent);
        Assert.Equal("--tab", parsed.File);
        Assert.Equal(new[] { "--compact-output" }, parsed.Operands);
    }

    [Theory]
    [InlineData("duplicate option `-c`", "-cc")]
    [InlineData("duplicate option `--compact-output`", "-c", "--compact-output")]
    [InlineData("duplicate option `-c`", "-c", "--tab", "-c")]
    [InlineData("option `--indent` expects a value", "--indent", "-c", "2")]
    [InlineData("unexpected option `-c`", "-cversion")]
    [InlineData("unknown option `--unknown`", "--unknown")]
    public void RetainsParseDiagnostics(string expectedMessage, params string[] arguments)
    {
        var directError = Assert.Throws<ParseException>(() => Parser.Parse<FormattingArgs>(arguments));
        var tokenizedError = Assert.Throws<ParseException>(() =>
            Parser.ParseTokenized<FormattingArgs>(Parser.Tokenize<FormattingArgs>(arguments)));

        Assert.Equal(expectedMessage, directError.Message);
        Assert.Equal(expectedMessage, tokenizedError.Message);
    }
}

[Arguments("Formatting options.")]
public partial record FormattingArgs
{
    [Argument("Compact output.", Short = 'c', Long = "compact-output")]
    public bool Compact { get; init; }

    [Argument("Tab indentation.", Long = "tab")]
    public bool Tab { get; init; }

    [Argument("Indentation width.", Long = "indent")]
    public int? Indent { get; init; }

    [Argument("Version information.", Short = 'V', Long = "version")]
    public bool Version { get; init; }

    [Argument("Filter file.", Short = 'f', Long = "from-file")]
    public string? File;

    [Argument("Operands.")]
    public string[] Operands { get; init; } = [];
}

[Arguments("Constructor options.")]
public partial record struct TokenizationConstructorArgs(
    [property: Argument("Item count.", LongNames = new[] { "count", "number" }, ShortNames = new[] { 'n', 'N' })] int Count)
{
    public static readonly TokenizationConstructorArgs Default =
        Parser.Parse<TokenizationConstructorArgs>(new[] { "--count=7" });
}
