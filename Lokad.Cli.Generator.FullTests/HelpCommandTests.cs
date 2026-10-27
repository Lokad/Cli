using System;
using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class HelpCommandTests
{
    [Theory]
    [InlineData("--help")]
    [InlineData("--usage")]
    [InlineData("-help")]
    [InlineData("-h")]
    [InlineData("-?")]
    [InlineData("-vh")]
    [InlineData("-hpvalue")]
    [InlineData("--help", "--count=bad")]
    [InlineData("--count=bad", "--help")]
    [InlineData("--pattern", "--help")]
    [InlineData("--format", "--help")]
    [InlineData("--help", "-7")]
    [InlineData("--help", "--help")]
    public void DetectsHelpBeforeBindingConversionAndConstruction(params string[] arguments) =>
        Assert.True(Parser.IsHelpRequested<HelpArgs>(arguments));

    [Theory]
    [InlineData()]
    [InlineData("file")]
    [InlineData("--count=bad")]
    [InlineData("--help=yes")]
    [InlineData("--pattern=--help")]
    [InlineData("--pattern", "--", "--help")]
    [InlineData("--", "--help")]
    [InlineData("-ph")]
    [InlineData("-p--help")]
    [InlineData("--raw", "--help")]
    [InlineData("-e", "--help")]
    [InlineData("--format=--help")]
    [InlineData("-hello")]
    [InlineData("-hello", "--", "--help")]
    public void ExcludesValuesAndDoesNotParseWhenHelpIsAbsent(params string[] arguments) =>
        Assert.False(Parser.IsHelpRequested<HelpArgs>(arguments));

    [Theory]
    [InlineData("--unknown", "--help")]
    [InlineData("--help", "--unknown")]
    [InlineData("--help", "-vz")]
    [InlineData("--help", "-h=bad")]
    public void GivesLexicalErrorsPrecedenceOverHelp(params string[] arguments) =>
        Assert.Throws<ParseException>(() => Parser.IsHelpRequested<HelpArgs>(arguments));

    [Fact]
    public void DoesNotChangeOrdinaryParseOrReserveUnmarkedNames()
    {
        Assert.Throws<ParseException>(() => Parser.Parse<HelpArgs>(new[] { "--help" }));
        Assert.False(Parser.IsHelpRequested<GroupedRawArgs>(new[] { "--help" }));
        Assert.Throws<ParseException>(() => Parser.IsHelpRequested<FormattingArgs>(new[] { "--help" }));
    }

    [Fact]
    public void ReusesTheSameCursorForInspectionAndBinding()
    {
        var tokens = Parser.Tokenize<GroupedHelpArgs>(new[] { "-pp", "one", "two" });
        var first = tokens.Peek();
        Assert.False(Parser.IsHelpRequested<GroupedHelpArgs>(tokens));
        Assert.Same(first, tokens.Peek());
        var parsed = Parser.ParseTokenized<GroupedHelpArgs>(tokens);
        Assert.Equal(new[] { "one", "two" }, parsed.Patterns);
        Assert.False(parsed.IsHelpRequested);
        Assert.Null(tokens.Peek());

        tokens = Parser.Tokenize<GroupedHelpArgs>(new[] { "--help" });
        first = tokens.Peek();
        Assert.True(Parser.IsHelpRequested<GroupedHelpArgs>(tokens));
        Assert.True(Parser.IsHelpRequested<GroupedHelpArgs>(tokens));
        Assert.Same(first, tokens.Peek());
        Assert.True(Parser.ParseTokenized<GroupedHelpArgs>(tokens).IsHelpRequested);
    }

    [Fact]
    public void RespectsLegacyGroupedValues()
    {
        Assert.False(Parser.IsHelpRequested<GroupedHelpArgs>(new[] { "-pp", "--", "one", "--help" }));
        Assert.True(Parser.IsHelpRequested<GroupedHelpArgs>(new[] { "-pp", "one", "two", "--help" }));
        Assert.False(Parser.IsHelpRequested<GroupedHelpArgs>(new[] { "-h=bad" }));
    }

    [Theory]
    [InlineData("--help")]
    [InlineData("--run", "--help")]
    [InlineData("tools", "run", "--help")]
    [InlineData("tools", "help")]
    [InlineData("--", "tools", "help")]
    [InlineData("--run", "tools", "help")]
    [InlineData("--tool-mode", "help")]
    [InlineData("help", "--tool-mode")]
    public void DetectsHelpChoicesBeforeMissingOperandsAndChoiceConflicts(params string[] arguments) =>
        Assert.True(Parser.IsHelpRequested<IHelpCommand>(arguments));

    [Theory]
    [InlineData("help")]
    [InlineData("tools", "run", "help")]
    [InlineData("--run", "help")]
    [InlineData("--tool-mode", "--target", "help")]
    [InlineData("--target", "tools", "help")]
    [InlineData("--help=yes")]
    [InlineData("--", "--help")]
    public void RequiresTheDeclaredHelpCommandPath(params string[] arguments) =>
        Assert.False(Parser.IsHelpRequested<IHelpCommand>(arguments));

    [Theory]
    [InlineData("--help")]
    [InlineData("--help=bad")]
    [InlineData("--help", "bad")]
    [InlineData("-hbad")]
    public void RecognizesValueTakingHelpDiscriminatorsWithoutConvertingTheirValues(params string[] arguments) =>
        Assert.True(Parser.IsHelpRequested<ValuedHelpChoice>(arguments));

    [Fact]
    public void SupportsNestedDefaultChoicesAndLeavesTheirCursorUntouched()
    {
        var tokens = Parser.Tokenize<IDefaultHelpRoot>(new[] { "help" });
        var first = tokens.Peek();
        Assert.True(Parser.IsHelpRequested<IDefaultHelpRoot>(tokens));
        Assert.Same(first, tokens.Peek());
        Assert.Throws<ParseException>(() => Parser.ParseTokenized<IDefaultHelpRoot>(tokens));
        Assert.Throws<ParseException>(() => Parser.Parse<IHelpCommand>(new[] { "--run", "--help" }));
    }

    [Fact]
    public void LetsExplicitCommandsOverrideTheDefaultHelpBranch()
    {
        Assert.True(Parser.IsHelpRequested<IHelpFallbackRoot>(new[] { "help" }));
        var arguments = new[] { "--run", "help" };
        Assert.False(Parser.IsHelpRequested<IHelpFallbackRoot>(arguments));
        Assert.Equal("help", Assert.IsType<RunHelpArgs>(Parser.Parse<IHelpFallbackRoot>(arguments)).Input);
    }

    [Fact]
    public void InspectsTheSelectedGroupWithoutRequiringItsOwnOptionDiscriminator()
    {
        Check<IFlagHelpCommand>("--tool-mode");
        Check<IShortHelpCommand>("-m");
        Check<ILongShortHelpCommand>("--tool-mode");

        static void Check<T>(string selector) where T : ICliParsed<T>
        {
            Assert.True(Parser.IsHelpRequested<T>(new[] { "help" }));
            Assert.True(Parser.IsHelpRequested<T>(new[] { selector, "help" }));
            Assert.False(Parser.IsHelpRequested<T>(new[] { "topic" }));
            Assert.Throws<ParseException>(() => Parser.Parse<T>(new[] { "help" }));

            var tokens = Parser.Tokenize<T>(new[] { "help", "topic" });
            var first = tokens.Peek();
            Assert.True(Parser.IsHelpRequested<T>(tokens));
            Assert.Same(first, tokens.Peek());
            Assert.Equal("topic", Assert.IsType<PrefixHelpArgs>(Parser.ParseTokenized<T>(tokens)).Target);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InspectsTheSelectedGroupWithItsOwnPrefixOptional(bool includePrefix)
    {
        var arguments = includePrefix ? new[] { "tools", "help", "topic" } : new[] { "help", "topic" };
        var tokens = Parser.Tokenize<IToolsHelpCommand>(arguments);
        var first = tokens.Peek();
        Assert.True(Parser.IsHelpRequested<IToolsHelpCommand>(tokens));
        Assert.Same(first, tokens.Peek());
        Assert.Equal("topic", Assert.IsType<PrefixHelpArgs>(Parser.ParseTokenized<IToolsHelpCommand>(tokens)).Target);
    }

    [Fact]
    public void RequiresAnOccurrenceOfTheEntrypointsOwnHelpPrefix()
    {
        Assert.True(Parser.IsHelpRequested<PrefixHelpArgs>(new[] { "help", "topic" }));
        Assert.False(Parser.IsHelpRequested<PrefixHelpArgs>(new[] { "topic" }));
        Assert.False(Parser.IsHelpRequested<PrefixHelpArgs>(Array.Empty<string>()));
        Assert.Equal("topic", Parser.Parse<PrefixHelpArgs>(new[] { "topic" }).Target);
    }

    [Fact]
    public void RespectsLiteralRemaindersAndNamedBoundaries()
    {
        Assert.False(Parser.IsHelpRequested<FirstOperandArgs>(new[] { "run", "file", "--help" }));
        Assert.True(Parser.IsHelpRequested<FirstOperandArgs>(new[] { "run", "--help", "file", "--unknown" }));
        Assert.False(Parser.IsHelpRequested<CodeRemainderArgs>(new[] { "--code", "code", "--help" }));
        Assert.True(Parser.IsHelpRequested<CodeRemainderArgs>(new[] { "--help", "--code=code", "--unknown" }));
        Assert.False(Parser.IsHelpRequested<CodeRemainderArgs>(new[] { "eval", "--", "--help" }));
    }
}

[Arguments("Inspect help without binding.", ShortOptionSyntax = ShortOptionSyntax.AttachedValues)]
public partial record HelpArgs
{
    public HelpArgs() => throw new InvalidOperationException("Help inspection must not construct the command.");
    [Argument("File.")] public string File { get; init; } = "";
    [Argument("Count.", Long = "count")] public HelpNeverValue? Count { get; init; }
    [Argument("Pattern.", Short = 'p', Long = "pattern")] public string? Pattern { get; init; }
    [Argument("Raw.", Short = 'e', Long = "raw", ConsumeNextValueVerbatim = true)] public string? Raw { get; init; }
    [Argument("Format.", Long = "format")] public OptionalValue<string> Format { get; init; }
    [Argument("Help.", LongNames = new[] { "help", "usage" }, ShortNames = new[] { 'h', '?' }, IsHelp = true)] public bool? Help;
    [Argument("Verbose.", Short = 'v')] public bool Verbose { get; init; }
    [Argument("Hello value.", Long = "hello")] public string? Hello { get; init; }
    [Argument("Limit.", ShortNumeric = true)] public int? Limit { get; init; }
}

public readonly record struct HelpNeverValue(int Value) : IParsable<HelpNeverValue>
{
    public static HelpNeverValue Parse(string text, IFormatProvider? provider) =>
        throw new InvalidOperationException("Help inspection must not convert values.");
    public static bool TryParse(string? text, IFormatProvider? provider, out HelpNeverValue value) =>
        throw new InvalidOperationException("Help inspection must not convert values.");
}

[Arguments("Grouped help flags.")]
public partial record GroupedHelpArgs(
    [property: Argument("Patterns.", Short = 'p')] string[] Patterns,
    [property: Argument("Help.", Long = "help", Short = 'h', IsHelp = true)] bool IsHelpRequested);

[Choice(typeof(RunHelpArgs))]
[Choice(typeof(HelpFlagChoice))]
[Choice(typeof(IToolsHelpCommand))]
[Choice(typeof(IFlagHelpCommand))]
public partial interface IHelpCommand { }

[Arguments("Run.", Long = "run")]
public partial record RunHelpArgs([property: Argument("Input.")] string Input) : IHelpCommand, IHelpFallbackRoot
{
    [Argument("Target.", Long = "target")] public string? Target { get; init; }
}

[Arguments("Help.", Long = "help", Short = 'h', IsHelp = true)]
public partial record HelpFlagChoice : IHelpCommand;

[Arguments("Help target.", Long = "help", Short = 'h', IsHelp = true, ShortOptionSyntax = ShortOptionSyntax.AttachedValues)]
public partial record ValuedHelpChoice(
    [property: Argument("Target.", Long = "help", Short = 'h')] HelpNeverValue Target);

[Arguments("Tools.", Prefix = "tools")]
[Choice(typeof(PrefixHelpArgs))]
[Choice(typeof(PrefixRunArgs))]
public partial interface IToolsHelpCommand : IHelpCommand { }

[Arguments("Tool mode.", Long = "tool-mode")]
[Choice(typeof(PrefixHelpArgs))]
public partial interface IFlagHelpCommand : IHelpCommand { }

[Arguments("Tools.", Short = 'm')]
[Choice(typeof(PrefixHelpArgs))]
public partial interface IShortHelpCommand { }

[Arguments("Tools.", Long = "tool-mode", Short = 'm')]
[Choice(typeof(PrefixHelpArgs))]
public partial interface ILongShortHelpCommand { }

[Arguments("Help.", Prefix = "help", IsHelp = true)]
public partial record PrefixHelpArgs([property: Argument("Target.")] string Target) :
    IToolsHelpCommand, IFlagHelpCommand, IDefaultHelpLayer, IShortHelpCommand, ILongShortHelpCommand;

[Arguments("Run.", Prefix = "run")]
public partial record PrefixRunArgs([property: Argument("File.")] string File) : IToolsHelpCommand;

[Choice(typeof(IDefaultHelpLayer))]
public partial interface IDefaultHelpRoot { }

[Choice(typeof(PrefixHelpArgs))]
public partial interface IDefaultHelpLayer : IDefaultHelpRoot, IHelpFallbackRoot { }

[Choice(typeof(RunHelpArgs))]
[Choice(typeof(IDefaultHelpLayer))]
public partial interface IHelpFallbackRoot { }
