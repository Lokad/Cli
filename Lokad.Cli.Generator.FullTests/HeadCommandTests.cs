using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class HeadCommandTests
{
    [Fact]
    public void ParsesSeparatedLineAndByteCounts()
    {
        var parsed = Parser.Parse<HeadArgs>(new[] { "-n", "5", "-c", "10", "file" });

        Assert.Equal(5, parsed.Lines);
        Assert.Equal(10, parsed.Bytes);
        Assert.Equal("file", parsed.Path);
    }

    [Theory]
    [InlineData("-10", 10)]
    [InlineData("-n10", 10)]
    public void ParsesCompactNumericForms(string argument, int expected)
    {
        var parsed = Parser.Parse<HeadArgs>(new[] { argument });

        Assert.Equal(expected, parsed.Lines);
    }
}

[Arguments("head command.")]
public partial record HeadArgs
{
    [Argument("Line count.", Short = 'n', ShortNumeric = true)]
    public int? Lines { get; init; }

    [Argument("Byte count.", Short = 'c')]
    public int? Bytes { get; init; }

    [Argument("Input path.", Name = "path")]
    public string? Path { get; init; }
}
