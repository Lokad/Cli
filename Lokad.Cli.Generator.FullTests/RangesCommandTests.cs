using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class RangesCommandTests
{
    [Fact]
    public void ParsesRangeOptionEndToEnd()
    {
        var parsed = Parser.Parse<RangesArgs>(new[] { "--ranges", "1,4,5-6,-10,20-" });

        Assert.Equal(5, parsed.Ranges.Count);
        Assert.Equal((1, 1), parsed.Ranges[0]);
        Assert.Equal((4, 4), parsed.Ranges[1]);
        Assert.Equal((5, 6), parsed.Ranges[2]);
        Assert.Equal((int.MinValue, 10), parsed.Ranges[3]);
        Assert.Equal((20, int.MaxValue), parsed.Ranges[4]);
    }

    [Theory]
    [InlineData("--ranges=invalid", "invalid value `invalid` for `--ranges`")]
    [InlineData("-r=1,,2", "invalid value `1,,2` for `-r`")]
    public void ReportsInvalidRangeOptionAsParseException(string argument, string expectedMessage)
    {
        var error = Assert.Throws<ParseException>(() => Parser.Parse<RangesArgs>(new[] { argument }));

        Assert.Equal(expectedMessage, error.Message);
    }
}

[Arguments("ranges command.")]
public partial record RangesArgs
{
    [Argument("Ranges.", Long = "ranges", Short = 'r')]
    public Ranges Ranges { get; init; } = new([]);
}
