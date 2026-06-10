using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class GzipCommandTests
{
    [Fact]
    public void ParsesCompressionOptionsAndInput()
    {
        var parsed = Parser.Parse<GzipArgs>(new[] { "-dkc9", "--keep", "--stdout", "file.gz" });

        Assert.True(parsed.Decompress);
        Assert.True(parsed.KeepShort);
        Assert.True(parsed.Keep);
        Assert.True(parsed.StdoutShort);
        Assert.True(parsed.Stdout);
        Assert.True(parsed.Best);
        Assert.Equal("file.gz", parsed.InputPath);
    }

    [Fact]
    public void ParsesLongAliases()
    {
        var parsed = Parser.Parse<GzipArgs>(new[] { "--uncompress", "--to-stdout", "--fast", "--best" });

        Assert.True(parsed.Uncompress);
        Assert.True(parsed.ToStdout);
        Assert.True(parsed.Fast);
        Assert.True(parsed.Best);
    }
}

[Arguments("gzip command.")]
public partial record GzipArgs
{
    [Argument("Decompresses.", Short = 'd', Long = "decompress")]
    public bool Decompress { get; init; }

    [Argument("Uncompresses.", Long = "uncompress")]
    public bool Uncompress { get; init; }

    [Argument("Keeps original.", Short = 'k')]
    public bool KeepShort { get; init; }

    [Argument("Keeps original.", Long = "keep")]
    public bool Keep { get; init; }

    [Argument("Writes to stdout.", Short = 'c')]
    public bool StdoutShort { get; init; }

    [Argument("Writes to stdout.", Long = "stdout")]
    public bool Stdout { get; init; }

    [Argument("Writes to stdout.", Long = "to-stdout")]
    public bool ToStdout { get; init; }

    [Argument("Fast preset.", Short = '1', Long = "fast")]
    public bool Fast { get; init; }

    [Argument("Compression preset.", Short = '2')]
    public bool Preset2 { get; init; }

    [Argument("Compression preset.", Short = '3')]
    public bool Preset3 { get; init; }

    [Argument("Compression preset.", Short = '4')]
    public bool Preset4 { get; init; }

    [Argument("Compression preset.", Short = '5')]
    public bool Preset5 { get; init; }

    [Argument("Compression preset.", Short = '6')]
    public bool Preset6 { get; init; }

    [Argument("Compression preset.", Short = '7')]
    public bool Preset7 { get; init; }

    [Argument("Compression preset.", Short = '8')]
    public bool Preset8 { get; init; }

    [Argument("Best preset.", Short = '9', Long = "best")]
    public bool Best { get; init; }

    [Argument("Input path.", Name = "path")]
    public string? InputPath { get; init; }
}
