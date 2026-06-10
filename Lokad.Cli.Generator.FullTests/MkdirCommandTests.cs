using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class MkdirCommandTests
{
    [Fact]
    public void ParsesParentsModeAndDirectory()
    {
        var parsed = Parser.Parse<MkdirArgs>(new[] { "-p", "-m", "755", "a" });

        Assert.True(parsed.Parents);
        Assert.Equal("755", parsed.Mode);
        Assert.Equal("a", parsed.Directory);
    }

    [Fact]
    public void RejectsLongOptions()
    {
        var ex = Assert.Throws<ParseException>(() => Parser.Parse<MkdirArgs>(new[] { "--parents", "a" }));

        Assert.Equal("unknown option `--parents`", ex.Message);
    }

    [Fact]
    public void RequiresDirectoryOperand()
    {
        var ex = Assert.Throws<ParseException>(() => Parser.Parse<MkdirArgs>(new[] { "-p" }));

        Assert.Equal("missing argument", ex.Message);
    }
}

[Arguments("mkdir command.")]
public partial record MkdirArgs
{
    [Argument("Creates parent directories.", Short = 'p')]
    public bool Parents { get; init; }

    [Argument("Mode value accepted by parser.", Short = 'm')]
    public string? Mode { get; init; }

    [Argument("Directory operand.", Name = "directory")]
    public string Directory { get; init; } = "";
}
