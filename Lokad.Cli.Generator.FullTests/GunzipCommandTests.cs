using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class GunzipCommandTests
{
    [Fact]
    public void ParsesSharedGzipOptions()
    {
        var parsed = Parser.Parse<GunzipArgs>(new[] { "-kc", "archive.gz" });

        Assert.True(parsed.KeepShort);
        Assert.True(parsed.StdoutShort);
        Assert.Equal("archive.gz", parsed.InputPath);
    }
}

[Arguments("gunzip command.")]
public partial record GunzipArgs
{
    [Argument("Keeps original.", Short = 'k', Long = "keep")]
    public bool KeepShort { get; init; }

    [Argument("Writes to stdout.", Short = 'c', Long = "stdout")]
    public bool StdoutShort { get; init; }

    [Argument("Writes to stdout.", Long = "to-stdout")]
    public bool ToStdout { get; init; }

    [Argument("Input path.", Name = "path")]
    public string? InputPath { get; init; }
}
