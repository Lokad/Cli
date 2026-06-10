using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class WcCommandTests
{
    [Fact]
    public void ParsesCountOptionsAndInputPath()
    {
        var parsed = Parser.Parse<WcArgs>(new[] { "-lwcm", "a" });

        Assert.True(parsed.Lines);
        Assert.True(parsed.Words);
        Assert.True(parsed.Bytes);
        Assert.True(parsed.Characters);
        Assert.Equal("a", parsed.Path);
    }
}

[Arguments("wc command.")]
public partial record WcArgs
{
    [Argument("Counts lines.", Short = 'l')]
    public bool Lines { get; init; }

    [Argument("Counts words.", Short = 'w')]
    public bool Words { get; init; }

    [Argument("Counts bytes.", Short = 'c')]
    public bool Bytes { get; init; }

    [Argument("Counts characters.", Short = 'm')]
    public bool Characters { get; init; }

    [Argument("Input path.", Name = "path")]
    public string? Path { get; init; }
}
