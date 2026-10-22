using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class RepeatedFlagCommandTests
{
    [Theory]
    [InlineData("-v")]
    [InlineData("--verbose")]
    [InlineData("-vv")]
    [InlineData("-v", "-v")]
    [InlineData("--verbose", "--verbose")]
    [InlineData("-v", "--verbose")]
    [InlineData("-verbose", "-vv")]
    public void AcceptsRepeatedFlags(params string[] arguments)
    {
        var parsed = Parser.Parse<RepeatedFlagArgs>(arguments);

        Assert.True(parsed.Verbose);
        Assert.False(parsed.Quiet);
        Assert.Null(parsed.Color);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(true, "-CC")]
    [InlineData(true, "--color", "-C")]
    public void RetainsNullableFlagAbsenceAndRepetition(bool? expected, params string[] arguments)
    {
        var parsed = Parser.Parse<RepeatedFlagArgs>(arguments);

        Assert.Equal(expected, parsed.Color);
        Assert.False(parsed.Verbose);
    }

    [Theory]
    [InlineData("duplicate option `-q`", "-vvqq")]
    [InlineData("duplicate option `--quiet`", "-vvq", "--quiet")]
    [InlineData("duplicate option `-q`", "--quiet", "-vvq")]
    public void RejectsStrictDuplicatesInMixedGroups(string expectedMessage, params string[] arguments)
    {
        var error = Assert.Throws<ParseException>(() => Parser.Parse<RepeatedFlagArgs>(arguments));

        Assert.Equal(expectedMessage, error.Message);
    }

    [Theory]
    [InlineData("--verbose=false", "unexpected option `--verbose`")]
    [InlineData("-v=false", "unexpected option `-v`")]
    [InlineData("--color=false", "unexpected option `--color`")]
    public void RejectsExplicitFlagValues(string argument, string expectedMessage)
    {
        var error = Assert.Throws<ParseException>(() => Parser.Parse<RepeatedFlagArgs>(new[] { argument }));

        Assert.Equal(expectedMessage, error.Message);
    }

    [Fact]
    public void PreservesInlineValuesAndTerminatedOperands()
    {
        var parsed = Parser.Parse<RepeatedFlagArgs>(new[]
        {
            "-vv", "--pattern=--color", "--", "-vv", "--color"
        });

        Assert.True(parsed.Verbose);
        Assert.Null(parsed.Color);
        Assert.Equal("--color", parsed.Pattern);
        Assert.Equal(new[] { "-vv", "--color" }, parsed.Operands);
    }

    [Fact]
    public void PreservesGroupedValueOptions()
    {
        var parsed = Parser.Parse<RepeatedFlagArgs>(new[] { "-evv", "--", "--color" });

        Assert.True(parsed.Verbose);
        Assert.Null(parsed.Color);
        Assert.Equal("--color", parsed.Pattern);
        Assert.Empty(parsed.Operands);
    }

    [Fact]
    public void BindsRepeatedFlagsThroughChoicesAndTokenizedParsing()
    {
        var tokens = Parser.Tokenize<IRepeatedFlagArgs>(new[] { "--run", "-vvC", "--color" });
        var parsed = Assert.IsType<RepeatedFlagArgs>(Parser.ParseTokenized<IRepeatedFlagArgs>(tokens));

        Assert.True(parsed.Verbose);
        Assert.True(parsed.Color);
    }

    [Fact]
    public void PreservesScalarOverwrite()
    {
        var parsed = Parser.Parse<RepeatedFlagArgs>(new[] { "-vv", "--pattern=first", "--pattern=second" });

        Assert.True(parsed.Verbose);
        Assert.Equal("second", parsed.Pattern);
    }

    [Fact]
    public void RequiresOptionValuesBeforeRepeatedFlags()
    {
        var error = Assert.Throws<ParseException>(() =>
            Parser.Parse<RepeatedFlagArgs>(new[] { "--pattern", "-vv" }));

        Assert.Equal("option `--pattern` expects a value", error.Message);
    }
}

[Choice(typeof(RepeatedFlagArgs))]
public partial interface IRepeatedFlagArgs
{
}

[Arguments("Repeated flag command.", Long = "run")]
public partial record RepeatedFlagArgs(
    [Argument("Enable verbose output.", Short = 'v', Long = "verbose", AllowRepeated = true)] bool Verbose)
    : IRepeatedFlagArgs
{
    [Argument("Enable color output.", Short = 'C', Long = "color", AllowRepeated = true)]
    public bool? Color;

    [Argument("Quiet output.", Short = 'q', Long = "quiet")]
    public bool Quiet { get; init; }

    [Argument("Pattern value.", Short = 'e', Long = "pattern")]
    public string? Pattern { get; init; }

    [Argument("Operands.")]
    public string[] Operands { get; init; } = [];
}
