using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class CatCommandTests
{
    [Fact]
    public void ParsesSupportedShortOptionsAndPath()
    {
        var parsed = Parser.Parse<CatArgs>(new[] { "-nbAs", "a.txt" });

        Assert.True(parsed.NumberAllLines);
        Assert.True(parsed.NumberNonBlankLines);
        Assert.True(parsed.ShowVisibleCharacters);
        Assert.True(parsed.SqueezeBlankLines);
        Assert.Equal("a.txt", parsed.Path);
    }

    [Fact]
    public void RejectsLongOptions()
    {
        var ex = Assert.Throws<ParseException>(() => Parser.Parse<CatArgs>(new[] { "--number" }));

        Assert.Equal("unknown option `--number`", ex.Message);
    }
}

[Arguments("cat command.")]
public partial record CatArgs
{
    [Argument("Numbers all lines.", Short = 'n')]
    public bool NumberAllLines { get; init; }

    [Argument("Numbers non-blank lines only.", Short = 'b')]
    public bool NumberNonBlankLines { get; init; }

    [Argument("Shows visible characters.", Short = 'A')]
    public bool ShowVisibleCharacters { get; init; }

    [Argument("Squeezes repeated blank lines.", Short = 's')]
    public bool SqueezeBlankLines { get; init; }

    [Argument("Input path.", Name = "path")]
    public string? Path { get; init; }
}
