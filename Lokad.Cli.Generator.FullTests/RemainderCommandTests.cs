using System;
using System.Collections.Generic;
using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class RemainderCommandTests
{
    [Theory]
    [InlineData("file", "--child-option", "-x")]
    [InlineData("file", "--", "--help")]
    [InlineData("file", "", "--inspect", "run")]
    [InlineData("file")]
    public void ForwardsTheSuffixAfterTheFirstOperand(params string[] arguments)
    {
        var parsed = Parser.Parse<FirstOperandArgs>(arguments);
        Assert.Equal("file", parsed.File);
        Assert.False(parsed.Help);
        Assert.Equal(arguments[1..], parsed.Tail);
    }

    [Fact]
    public void ConsumesOptionValuesAndCommandPrefixesBeforeCountingOperands()
    {
        var parsed = Assert.IsType<FirstOperandArgs>(Parser.Parse<IRemainderCommand>(
            new[] { "run", "-5", "-v", "--output", "destination", "file", "--help", "-8" }));
        Assert.True(parsed.Verbose);
        Assert.Equal(5, parsed.Limit);
        Assert.Equal("destination", parsed.Output);
        Assert.Equal("file", parsed.File);
        Assert.Equal(new[] { "--help", "-8" }, parsed.Tail);

        parsed = Parser.Parse<FirstOperandArgs>(new[] { "--output=destination", "--", "file", "--help" });
        Assert.Equal("destination", parsed.Output);
        Assert.Equal(new[] { "--help" }, parsed.Tail);
    }

    [Fact]
    public void KeepsLiteralTokensOutOfAllSelectorInspection()
    {
        var tokens = Parser.Tokenize<IRemainderCommand>(new[] { "run", "file", "--help", "--inspect", "-i", "--", "run" });
        Assert.False(tokens.HasLongOption("help"));
        Assert.False(tokens.HasLongOption("inspect"));
        Assert.False(tokens.HasShortOption('i'));
        Assert.True(tokens.HasPositionalPrefix("run"));
        Assert.Equal(new ValueArg("run"), tokens.Shift());
        Assert.Equal(new ValueArg("file"), tokens.Shift());
        foreach (var value in new[] { "--help", "--inspect", "-i", "--", "run" })
            Assert.Equal(new LiteralArg(value), tokens.Shift());
        Assert.Null(tokens.Peek());

        var parsed = Assert.IsType<FirstOperandArgs>(Parser.Parse<IRemainderCommand>(
            new[] { "run", "file", "--inspect", "-i" }));
        Assert.Equal(new[] { "--inspect", "-i" }, parsed.Tail);
    }

    [Theory]
    [InlineData("--unknown", "file")]
    [InlineData("-x", "file")]
    [InlineData("--output", "--help", "file")]
    public void RejectsInvalidOptionsBeforeTheBoundary(params string[] arguments) =>
        Assert.Throws<ParseException>(() => Parser.Parse<FirstOperandArgs>(arguments));

    [Fact]
    public void StartsAnExplicitRemainderAfterAllDeclaredSelectors()
    {
        var parsed = Parser.Parse<SelectedRemainderArgs>(new[] { "7", "--verbose", "file", "--help", "--", "" });
        Assert.Equal(7, parsed.Job);
        Assert.Equal("file", parsed.File);
        Assert.True(parsed.Verbose);
        Assert.Equal(new[] { "--help", "--", "" }, parsed.Tail.ToArray());
        Assert.Empty(Parser.Parse<SelectedRemainderArgs>(new[] { "7", "file" }).Tail.ToArray());
        Assert.Throws<ParseException>(() => Parser.Parse<SelectedRemainderArgs>(new[] { "bad", "file" }));
    }

    [Theory]
    [InlineData("--code", "code")]
    [InlineData("--code=code")]
    [InlineData("-c", "code")]
    [InlineData("-ccode")]
    [InlineData("-c=code")]
    [InlineData("--eval", "code")]
    [InlineData("-code", "code")]
    [InlineData("--code", "--", "code")]
    public void ConsumesTheBoundaryOptionsValueBeforeForwarding(params string[] option)
    {
        var arguments = new List<string>(option) { "--help", "--inspect", "--", "-5", "eval" };
        var tokens = Parser.Tokenize<CodeRemainderArgs>(arguments);
        Assert.False(tokens.HasLongOption("help"));
        Assert.False(tokens.HasLongOption("inspect"));
        Assert.False(tokens.HasPositionalPrefix("eval"));
        var parsed = Parser.ParseTokenized<CodeRemainderArgs>(tokens);
        Assert.Equal("code", parsed.Code);
        Assert.False(parsed.Help);
        Assert.Equal(new[] { "--help", "--inspect", "--", "-5", "eval" }, parsed.Tail);
    }

    [Fact]
    public void HandlesEmptyAndVerbatimOptionBoundaries()
    {
        var empty = Parser.Parse<CodeRemainderArgs>(new[] { "--code=" });
        Assert.Empty(empty.Tail);
        Assert.Equal("", empty.Code);
        Assert.Empty(Parser.Parse<CodeRemainderArgs>(new[] { "--" }).Tail);
        var parsed = Parser.Parse<RawBoundaryArgs>(new[] { "--code", "--", "--help", "--" });
        Assert.Equal("--", parsed.Code);
        Assert.Equal(new[] { "--help", "--" }, parsed.Tail);
        var grouped = Parser.Parse<CodeRemainderArgs>(new[] { "-hc", "code", "--help" });
        Assert.True(grouped.Help);
        Assert.Equal("code", grouped.Code);
        Assert.Equal(new[] { "--help" }, grouped.Tail);
    }

    [Theory]
    [InlineData("--", "--help", "-h", "--", "eval")]
    [InlineData("eval", "--", "--help", "-h", "--", "eval")]
    [InlineData("--", "eval", "--help", "-h", "--", "eval")]
    public void StartsADeclaredRemainderWithTheNormalTerminator(params string[] arguments)
    {
        var tokens = Parser.Tokenize<CodeRemainderArgs>(arguments);
        Assert.False(tokens.HasLongOption("help"));
        Assert.False(tokens.HasShortOption('h'));
        var parsed = Parser.ParseTokenized<CodeRemainderArgs>(tokens);

        Assert.Null(parsed.Code);
        Assert.False(parsed.Help);
        Assert.Equal(new[] { "--help", "-h", "--", "eval" }, parsed.Tail);
        Assert.Null(tokens.Peek());
    }

    [Theory]
    [InlineData("eval", "--", "--help", "--")]
    [InlineData("--", "eval", "--help", "--")]
    public void PreservesCommandSelectionBeforeADeclaredRemainder(params string[] arguments)
    {
        var parsed = Assert.IsType<CodeRemainderArgs>(Parser.Parse<ICodeRemainderCommand>(arguments));
        Assert.Null(parsed.Code);
        Assert.False(parsed.Help);
        Assert.Equal(new[] { "--help", "--" }, parsed.Tail);
    }

    [Theory]
    [InlineData("--code")]
    [InlineData("-c")]
    [InlineData("--code", "--help")]
    [InlineData("operand", "--code=x")]
    public void ReportsMissingBoundaryValuesAndPrematureRemainders(params string[] arguments) =>
        Assert.Throws<ParseException>(() => Parser.Parse<CodeRemainderArgs>(arguments));

    [Fact]
    public void KeepsDefaultInterspersedOptionsAndOrdinaryTerminators()
    {
        var parsed = Parser.Parse<GroupedRawArgs>(new[] { "file", "--help", "--", "--child" });
        Assert.True(parsed.Help);
        Assert.Equal(new[] { "file", "--child" }, parsed.Input);
    }

    [Fact]
    public void RecognizesNestedPrefixesAndOptionSelectorsBeforeTheOperand()
    {
        var parsed = Assert.IsType<FirstOperandArgs>(Parser.Parse<INestedRemainderCommand>(
            new[] { "tools", "run", "--help", "file", "--inspect" }));
        Assert.True(parsed.Help);
        Assert.Equal(new[] { "--inspect" }, parsed.Tail);
        var flagged = Assert.IsType<FlagRemainderArgs>(Parser.Parse<IFlagRemainderCommand>(
            new[] { "--execute", "file", "--other" }));
        Assert.Equal(new[] { "--other" }, flagged.Tail);
        Assert.Throws<ParseException>(() => Parser.Parse<IFlagRemainderCommand>(
            new[] { "--execute", "--other", "file" }));
    }

    [Fact]
    public void DoesNotCountAnOptionalValueOccurrenceAsAnOperand()
    {
        var parsed = Parser.Parse<FirstOperandArgs>(new[] { "--format", "file", "--format=json" });
        Assert.True(parsed.Format.IsPresent);
        Assert.False(parsed.Format.HasValue);
        Assert.Equal("file", parsed.File);
        Assert.Equal(new[] { "--format=json" }, parsed.Tail);
    }

    [Fact]
    public void AllowsTheFirstOperandItselfToBeTheRemainder()
    {
        var parsed = Parser.Parse<OnlyRemainderArgs>(new[] { "first", "--help", "--" });
        Assert.Equal(new[] { "first", "--help", "--" }, parsed.Tail);
        Assert.Empty(Parser.Parse<OnlyRemainderArgs>(Array.Empty<string>()).Tail);
        parsed = Parser.Parse<OnlyRemainderArgs>(new[] { "-pp", "one", "two", "first", "--help" });
        Assert.Equal(new[] { "one", "two" }, parsed.Values);
        Assert.Equal(new[] { "first", "--help" }, parsed.Tail);
    }

    [Fact]
    public void ResolvesNestedSelectorsAndRetainsTheirOptions()
    {
        foreach (var arguments in new[]
        {
            new[] { "tools", "--execute", "--code", "text", "--help", "tools" },
            new[] { "--execute", "tools", "--code=text", "--help", "tools" }
        })
        {
            var parsed = Assert.IsType<NestedCodeArgs>(Parser.Parse<IBoundaryRoot>(arguments));
            Assert.Equal("text", parsed.Code);
            Assert.Equal(new[] { "--help", "tools" }, parsed.Tail);
        }
        Assert.Throws<ParseException>(() => Parser.Tokenize<IBoundaryRoot>(
            new[] { "--code=text", "tools", "--execute" }));
    }

    [Fact]
    public void AppliesEachSelectedCommandsOwnOperandCount()
    {
        var selected = Assert.IsType<SelectedRemainderArgs>(Parser.Parse<ILayoutCommand>(
            new[] { "selected", "7", "--verbose", "file", "--unknown" }));
        Assert.Equal(new[] { "--unknown" }, selected.Tail.ToArray());
        var fallback = Assert.IsType<DefaultRemainderArgs>(Parser.Parse<ILayoutCommand>(
            new[] { "file", "--unknown" }));
        Assert.Equal(new[] { "--unknown" }, fallback.Tail);
    }

    [Fact]
    public void StillConvertsARequiredBoundaryValue()
    {
        var parsed = Parser.Parse<NumberBoundaryArgs>(new[] { "--number=7", "--unknown" });
        Assert.Equal(7, parsed.Number);
        Assert.Equal(new[] { "--unknown" }, parsed.Tail);
        Assert.Throws<ParseException>(() => Parser.Parse<NumberBoundaryArgs>(new[] { "--number=bad", "--unknown" }));
        Assert.Throws<ParseException>(() => Parser.Parse<NumberBoundaryArgs>(Array.Empty<string>()));
    }
}

[Arguments("Run a file.", Prefix = "run", StopAtFirstOperand = true, ShortOptionSyntax = ShortOptionSyntax.AttachedValues)]
public partial record FirstOperandArgs(
    [property: Argument("File.")] string File,
    [property: Argument("Forwarded arguments.")] string[] Tail) : IRemainderCommand, INestedRemainderCommand
{
    [Argument("Verbose.", Short = 'v')] public bool Verbose { get; init; }
    [Argument("Help.", Long = "help", IsHelp = true)] public bool Help { get; init; }
    [Argument("Output.", Long = "output")] public string? Output { get; init; }
    [Argument("Limit.", ShortNumeric = true)] public int? Limit { get; init; }
    [Argument("Format.", Long = "format")] public OptionalValue<string> Format { get; init; }
}

[Choice(typeof(FirstOperandArgs))]
[Choice(typeof(InspectRemainderArgs))]
public partial interface IRemainderCommand { }

[Arguments("Nested commands.", Prefix = "tools")]
[Choice(typeof(FirstOperandArgs))]
public partial interface INestedRemainderCommand { }

[Arguments("Inspect.", Long = "inspect", Short = 'i')]
public partial record InspectRemainderArgs : IRemainderCommand;

[Arguments("Forward arguments after a job and file.", Prefix = "selected")]
public partial record SelectedRemainderArgs(
    [property: Argument("Job.")] int Job,
    [property: Argument("File.")] string File,
    [property: Argument("Forwarded arguments.", Remainder = true)] ReadOnlyMemory<string> Tail) : ILayoutCommand
{
    [Argument("Verbose.", Long = "verbose")] public bool Verbose { get; init; }
}

[Arguments("Evaluate code.", Prefix = "eval", ShortOptionSyntax = ShortOptionSyntax.AttachedValues)]
public partial record CodeRemainderArgs : ICodeRemainderCommand
{
    [Argument("Code.", LongNames = new[] { "code", "eval" }, Short = 'c', StopOptionParsing = true)]
    public string? Code { get; init; }
    [Argument("Forwarded arguments.", Remainder = true)] public IReadOnlyList<string> Tail { get; init; } = Array.Empty<string>();
    [Argument("Help.", Long = "help", Short = 'h', IsHelp = true)] public bool Help { get; init; }
}

[Choice(typeof(CodeRemainderArgs))]
public partial interface ICodeRemainderCommand { }

[Arguments("Raw code.")]
public partial record RawBoundaryArgs(
    [property: Argument("Code.", Long = "code", StopOptionParsing = true, ConsumeNextValueVerbatim = true)] string Code,
    [property: Argument("Forwarded arguments.")] string[] Tail);

[Choice(typeof(FlagRemainderArgs))]
[Choice(typeof(OtherFlagRemainderArgs))]
public partial interface IFlagRemainderCommand { }

[Arguments("Other.", Long = "other")]
public partial record OtherFlagRemainderArgs : IFlagRemainderCommand;

[Arguments("Execute.", Long = "execute")]
public partial record FlagRemainderArgs(
    [property: Argument("File.")] string File,
    [property: Argument("Forwarded arguments.", Remainder = true)] string[] Tail) : IFlagRemainderCommand;

[Arguments("Forward operands.", StopAtFirstOperand = true)]
public partial record OnlyRemainderArgs(
    [property: Argument("Forwarded arguments.", Remainder = true)] string[] Tail)
{
    [Argument("Option values.", Short = 'p')] public string[] Values { get; init; } = Array.Empty<string>();
}

[Choice(typeof(IToolBoundaryCommand))]
public partial interface IBoundaryRoot { }

[Arguments("Tools.", Prefix = "tools")]
[Choice(typeof(NestedCodeArgs))]
public partial interface IToolBoundaryCommand : IBoundaryRoot { }

[Arguments("Execute.", Long = "execute")]
public partial record NestedCodeArgs(
    [property: Argument("Code.", Long = "code", StopOptionParsing = true)] string Code,
    [property: Argument("Forwarded arguments.", Remainder = true)] string[] Tail) : IToolBoundaryCommand;

[Choice(typeof(SelectedRemainderArgs))]
[Choice(typeof(DefaultRemainderArgs))]
public partial interface ILayoutCommand { }

[Arguments("Default.")]
public partial record DefaultRemainderArgs(
    [property: Argument("File.")] string File,
    [property: Argument("Forwarded arguments.", Remainder = true)] string[] Tail) : ILayoutCommand;

[Arguments("Typed boundary.")]
public partial record NumberBoundaryArgs(
    [property: Argument("Number.", Long = "number", StopOptionParsing = true)] int Number,
    [property: Argument("Forwarded arguments.")] string[] Tail);
