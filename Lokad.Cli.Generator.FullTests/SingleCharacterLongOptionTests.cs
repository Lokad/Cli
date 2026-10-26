using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class SingleCharacterLongOptionTests
{
    [Theory]
    [InlineData("-x", "7")]
    [InlineData("-x=7")]
    [InlineData("--x", "7")]
    [InlineData("--x=7")]
    public void AcceptsSeparateAndAttachedValues(params string[] arguments)
    {
        Assert.Equal(7, Parser.Parse<SingleLongArgs>(arguments).Value);
    }

    [Theory]
    [InlineData("-v")]
    [InlineData("--v")]
    public void AcceptsSingleCharacterLongFlags(string argument)
    {
        Assert.True(Parser.Parse<SingleLongFlagArgs>(new[] { argument }).Verbose);
    }

    [Fact]
    public void ReservesOneDashForShortOptionsWhenConfigured()
    {
        var shortForm = Parser.Parse<SingleLongShortOnlyArgs>(new[] { "-x" });
        Assert.True(shortForm.ShortValue);
        Assert.Null(shortForm.LongValue);
        var longForm = Parser.Parse<SingleLongShortOnlyArgs>(new[] { "--x", "7" });
        Assert.False(longForm.ShortValue);
        Assert.Equal(7, longForm.LongValue);
    }
}

[Arguments("Single long value.")]
public partial record SingleLongArgs([property: Argument("Value.", Long = "x")] int Value);

[Arguments("Single long flag.")]
public partial record SingleLongFlagArgs([property: Argument("Verbose.", Long = "v")] bool Verbose);

[Arguments("One dash selects short options.", AllowSingleDashLongOptions = false)]
public partial record SingleLongShortOnlyArgs
{
    [Argument("Long value.", Long = "x")] public int? LongValue { get; init; }
    [Argument("Short flag.", Short = 'x')] public bool ShortValue { get; init; }
}
