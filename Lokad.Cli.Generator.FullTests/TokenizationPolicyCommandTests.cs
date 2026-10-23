using System;
using System.Collections.Generic;
using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class TokenizationPolicyCommandTests
{
    [Theory]
    [InlineData(true, "pattern", "-fepattern")]
    [InlineData(false, "f", "-ef")]
    [InlineData(true, "pattern", "-fe", "pattern")]
    [InlineData(false, "pattern", "-e", "pattern")]
    [InlineData(true, "", "-fe=")]
    [InlineData(false, "", "-e=")]
    [InlineData(true, "foo=bar", "-fefoo=bar")]
    [InlineData(false, "=text", "-e==text")]
    [InlineData(false, "e", "-ee")]
    public void SplitsFlagPrefixesFromAttachedValues(bool flag, string expression, params string[] arguments)
    {
        var parsed = Parser.Parse<AttachedOptionArgs>(arguments);

        Assert.Equal(flag, parsed.Flag);
        Assert.Equal(expression, parsed.Expression);
        Assert.Empty(parsed.Input);
    }

    [Theory]
    [InlineData(false, "f", "-pf")]
    [InlineData(true, "text", "-fptext")]
    [InlineData(true, "text", "-fp", "text")]
    [InlineData(false, "", "-p=")]
    public void SupportsAttachedValuesWithoutVerbatimOperands(bool flag, string expected, params string[] arguments)
    {
        var parsed = Parser.Parse<AttachedOptionArgs>(arguments);
        Assert.Equal(flag, parsed.Flag);
        Assert.Equal(expected, parsed.Plain);
    }

    [Fact]
    public void EnablesAttachedSyntaxIndependentlyAndNormalizesConsumedOperands()
    {
        var tokens = Parser.Tokenize<AttachedOnlyArgs>(new[] { "-fe", "f" });
        Assert.Equal(new ShortArgSet("f"), tokens.Shift());
        Assert.Equal(new ShortArgWithValue('e', "f"), tokens.Shift());
        Assert.Null(tokens.Peek());
        Assert.Equal("f", Parser.Parse<AttachedOnlyArgs>(new[] { "-ef" }).Expression);
        Assert.Equal("value", Parser.Parse<AttachedOnlyArgs>(new[] { "-expression=value" }).Expression);
    }

    [Theory]
    [InlineData("--help", "-e", "--help")]
    [InlineData("--", "--expression", "--")]
    [InlineData("-unknown", "-fe", "-unknown")]
    [InlineData("", "--expr", "")]
    [InlineData("--help", "--expression=--help")]
    [InlineData("--help", "-expression", "--help")]
    public void KeepsVerbatimOperandsOutOfOptionInspection(string expected, params string[] arguments)
    {
        var tokens = Parser.Tokenize<AttachedOptionArgs>(arguments);
        Assert.False(tokens.HasLongOption("help"));
        Assert.False(tokens.HasShortOption('h'));
        var parsed = Parser.ParseTokenized<AttachedOptionArgs>(tokens);

        Assert.Equal(expected, parsed.Expression);
        Assert.False(parsed.Help);
        Assert.Null(tokens.Peek());
    }

    [Theory]
    [InlineData(-2, "--number", "-2")]
    [InlineData(-2, "--num", "-2")]
    [InlineData(-2, "-n", "-2")]
    [InlineData(-2, "-fn-2")]
    [InlineData(-2, "--number=-2")]
    public void ConvertsDashPrefixedRawValues(int expected, params string[] arguments) =>
        Assert.Equal(expected, Parser.Parse<AttachedOptionArgs>(arguments).Number);

    [Fact]
    public void KeepsTheTerminatorLiteralOnlyForTheRawOperand()
    {
        var parsed = Parser.Parse<AttachedOptionArgs>(new[] { "-e", "--", "--help", "--", "--number" });

        Assert.Equal("--", parsed.Expression);
        Assert.True(parsed.Help);
        Assert.Equal(new[] { "--number" }, parsed.Input);
    }

    [Fact]
    public void KeepsNormalValueAndNumericShortRules()
    {
        var parsed = Parser.Parse<AttachedOptionArgs>(new[] { "-fh", "-p", "--", "-2" });
        Assert.True(parsed.Flag);
        Assert.True(parsed.Help);
        Assert.Equal("-2", parsed.Plain);
        Assert.Null(parsed.Limit);

        parsed = Parser.Parse<AttachedOptionArgs>(new[] { "-5", "-n", "-2" });
        Assert.Equal(5, parsed.Limit);
        Assert.Equal(-2, parsed.Number);
    }

    [Theory]
    [InlineData("option `--expression` expects a value", "-fe")]
    [InlineData("option `--expression` expects a value", "--expr")]
    [InlineData("invalid value `bad` for `--number`", "--num", "bad")]
    [InlineData("invalid value `--` for `--number`", "--number", "--")]
    [InlineData("unknown short option `-z`", "-fz")]
    [InlineData("unknown option `--unknown`", "--unknown")]
    [InlineData("option `-p` expects a value", "-p", "--help")]
    public void ReportsTypedAndLexicalErrors(string message, params string[] arguments) =>
        Assert.Equal(message, Assert.Throws<ParseException>(() => Parser.Parse<AttachedOptionArgs>(arguments)).Message);

    [Fact]
    public void SupportsVerbatimOperandsInLegacyGroups()
    {
        var parsed = Parser.Parse<GroupedRawArgs>(new[] { "-ee", "--help", "--", "tail" });
        Assert.Equal(new[] { "--help", "--" }, parsed.Expressions);
        Assert.False(parsed.Help);
        Assert.Equal(new[] { "tail" }, parsed.Input);

        parsed = Parser.Parse<GroupedRawArgs>(new[] { "-pe", "plain", "--help" });
        Assert.Equal(new[] { "plain" }, parsed.Plain);
        Assert.Equal(new[] { "--help" }, parsed.Expressions);

        parsed = Parser.Parse<GroupedRawArgs>(new[] { "-pe", "--", "plain", "--help" });
        Assert.Equal(new[] { "plain" }, parsed.Plain);
        Assert.Equal(new[] { "--help" }, parsed.Expressions);
    }

    [Fact]
    public void LeavesLegacySyntaxAndDefaultDashRulesUnchanged()
    {
        var grouped = Parser.Parse<GroupedRawArgs>(new[] { "-pp", "first", "second" });
        Assert.Equal(new[] { "first", "second" }, grouped.Plain);
        Assert.Equal("unknown short option `-2`", Assert.Throws<ParseException>(() =>
            Parser.Parse<MyArgs>(new[] { "--max-depth", "-2" })).Message);
    }

    [Fact]
    public void CanDisableSingleDashLongNamesIndependently()
    {
        var parsed = Parser.Parse<ShortOnlyArgs>(new[] { "short", "-all" });
        Assert.True(parsed.A);
        Assert.True(parsed.L);
        Assert.False(parsed.All);
        Assert.True(Parser.Parse<ShortOnlyArgs>(new[] { "short", "--all" }).All);
    }

    [Fact]
    public void UsesTheSameSchemaAcrossChoicesAndStandaloneParsing()
    {
        var arguments = new[] { "find", "-fef", "--number", "-2" };
        var parsed = Assert.IsType<FindPolicyArgs>(Parser.Parse<IPolicyCommand>(arguments));
        Assert.True(parsed.Flag);
        Assert.Equal("f", parsed.Expression);
        Assert.Equal(-2, parsed.Number);
        var direct = Parser.Parse<FindPolicyArgs>(arguments);
        Assert.Equal(parsed, direct);

        var tokens = Parser.Tokenize<IPolicyCommand>(new[] { "find", "-e", "--other" });
        Assert.False(tokens.HasLongOption("other"));
        Assert.IsType<FindPolicyArgs>(Parser.ParseTokenized<IPolicyCommand>(tokens));

        var other = Assert.IsType<OtherPolicyArgs>(Parser.Parse<IPolicyCommand>(new[] { "-e", "find", "--other" }));
        Assert.Equal("find", other.Expression);
    }

    [Fact]
    public void SharesAttachedSyntaxAcrossChoiceParents()
    {
        var arguments = new[] { "find", "-ef" };
        var direct = Parser.Parse<FindPolicyArgs>(arguments);
        var configured = Assert.IsType<FindPolicyArgs>(Parser.Parse<IPolicyCommand>(arguments));
        var inherited = Assert.IsType<FindPolicyArgs>(Parser.Parse<IUnconfiguredPolicyCommand>(arguments));
        var tokenized = Assert.IsType<FindPolicyArgs>(Parser.ParseTokenized<IUnconfiguredPolicyCommand>(
            Parser.Tokenize<IUnconfiguredPolicyCommand>(arguments)));

        Assert.Equal("f", direct.Expression);
        Assert.Equal(direct, configured);
        Assert.Equal(direct, inherited);
        Assert.Equal(direct, tokenized);
        Assert.IsType<OtherPolicyArgs>(Parser.Parse<IUnconfiguredPolicyCommand>(new[] { "--other" }));
        Assert.Throws<ParseException>(() => Parser.Parse<IUnconfiguredPolicyCommand>(Array.Empty<string>()));
    }

    [Fact]
    public void SharesSingleDashPolicyAcrossChoiceParents()
    {
        var arguments = new[] { "short", "-all" };
        var direct = Parser.Parse<ShortOnlyArgs>(arguments);
        var configured = Assert.IsType<ShortOnlyArgs>(Parser.Parse<IShortOnlyCommand>(arguments));
        var inherited = Assert.IsType<ShortOnlyArgs>(Parser.Parse<IUnconfiguredShortCommand>(arguments));

        Assert.True(direct.A && direct.L);
        Assert.False(direct.All);
        Assert.Equal(direct, configured);
        Assert.Equal(direct, inherited);
        Assert.Equal(direct, Parser.ParseTokenized<IUnconfiguredShortCommand>(
            Parser.Tokenize<IUnconfiguredShortCommand>(arguments)));
        Assert.True(Assert.IsType<ShortOnlyArgs>(Parser.Parse<IUnconfiguredShortCommand>(new[] { "short", "--all" })).All);
    }

    [Fact]
    public void IgnoresChoiceInterfaceMemberAnnotations()
    {
        var parsed = Assert.IsType<FindPolicyArgs>(Parser.Parse<IPolicyCommand>(new[] { "find", "-ef" }));
        Assert.Equal("f", parsed.Expression);
        Assert.Equal("unknown option `--ignored-interface`", Assert.Throws<ParseException>(() =>
            Parser.Tokenize<IPolicyCommand>(new[] { "find", "--ignored-interface" })).Message);
    }
}

[Arguments("Attached option values.", ShortOptionSyntax = ShortOptionSyntax.AttachedValues)]
public partial record AttachedOptionArgs
{
    [Argument("Flag.", Short = 'f')] public bool Flag { get; init; }
    [Argument("Expression.", Short = 'e', LongNames = new[] { "expression", "expr" }, ConsumeNextValueVerbatim = true)]
    public string? Expression { get; init; }
    [Argument("Number.", Short = 'n', LongNames = new[] { "number", "num" }, ConsumeNextValueVerbatim = true)]
    public int? Number { get; init; }
    [Argument("Normal operand.", Short = 'p')] public string? Plain { get; init; }
    [Argument("Help.", Short = 'h', Long = "help")] public bool Help { get; init; }
    [Argument("Numeric short value.", ShortNumeric = true)] public int? Limit { get; init; }
    [Argument("Input.")] public string[] Input { get; init; } = [];
}

[Arguments("Grouped raw operands.")]
public partial record GroupedRawArgs
{
    [Argument("Expression.", Short = 'e', ConsumeNextValueVerbatim = true)] public IReadOnlyList<string> Expressions { get; init; } = Array.Empty<string>();
    [Argument("Normal operand.", Short = 'p')] public string[] Plain { get; init; } = [];
    [Argument("Help.", Long = "help")] public bool Help { get; init; }
    [Argument("Input.")] public string[] Input { get; init; } = [];
}

[Arguments("Attached syntax alone.", ShortOptionSyntax = ShortOptionSyntax.AttachedValues)]
public partial record AttachedOnlyArgs(
    [property: Argument("Expression.", Short = 'e', Long = "expression")] string? Expression)
{
    [Argument("Flag.", Short = 'f')] public bool Flag { get; init; }
}

[Arguments("Short options only for one dash.", Prefix = "short")]
public partial record ShortOnlyArgs : IShortOnlyCommand, IUnconfiguredShortCommand
{
    [Argument("A.", Short = 'a')] public bool A { get; init; }
    [Argument("L.", Short = 'l', AllowRepeated = true)] public bool L { get; init; }
    [Argument("All.", Long = "all")] public bool All { get; init; }
}

[Arguments("Short options for one dash.", AllowSingleDashLongOptions = false)]
[Choice(typeof(ShortOnlyArgs))]
public partial interface IShortOnlyCommand { }

[Choice(typeof(ShortOnlyArgs))]
public partial interface IUnconfiguredShortCommand { }

[Arguments("Value-aware commands.", ShortOptionSyntax = ShortOptionSyntax.AttachedValues)]
[Choice(typeof(FindPolicyArgs))]
[Choice(typeof(OtherPolicyArgs))]
public partial interface IPolicyCommand
{
    [Argument("Ignored interface member.", Short = 'e', Long = "ignored-interface")]
    bool IgnoredExpression { get; }
}

[Choice(typeof(OtherPolicyArgs))]
[Choice(typeof(FindPolicyArgs))]
public partial interface IUnconfiguredPolicyCommand { }

[Arguments("Find.", Prefix = "find")]
public partial record FindPolicyArgs : IPolicyCommand, IUnconfiguredPolicyCommand
{
    public bool IgnoredExpression => false;
    [Argument("Flag.", Short = 'f')] public bool Flag { get; init; }
    [Argument("Expression.", Short = 'e', ConsumeNextValueVerbatim = true)] public string? Expression { get; init; }
    [Argument("Number.", Long = "number", ConsumeNextValueVerbatim = true)] public int? Number { get; init; }
}

[Arguments("Other.", Long = "other")]
public partial record OtherPolicyArgs : IPolicyCommand, IUnconfiguredPolicyCommand
{
    public bool IgnoredExpression => false;
    [Argument("Expression.", Short = 'e', ConsumeNextValueVerbatim = true)] public string? Expression { get; init; }
}
