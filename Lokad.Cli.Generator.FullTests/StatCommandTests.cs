using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class StatCommandTests
{
    [Fact]
    public void ParsesShortAndLongFormatOptions()
    {
        var parsed = Parser.Parse<StatArgs>(new[] { "-c", "%n", "--printf=%s", "--cached=always", "--terse", "file" });

        Assert.Equal("%n", parsed.FormatShort);
        Assert.Equal("%s", parsed.Printf);
        Assert.Equal("always", parsed.Cached);
        Assert.True(parsed.Terse);
        Assert.Equal("file", parsed.Path);
    }

    [Fact]
    public void ParsesSeparatedLongFormatValue()
    {
        var parsed = Parser.Parse<StatArgs>(new[] { "--format", "%n" });

        Assert.Equal("%n", parsed.Format);
    }
}

[Arguments("stat command.")]
public partial record StatArgs
{
    [Argument("Cached mode.", Long = "cached")]
    public string? Cached { get; init; }

    [Argument("File system status.", Short = 'f', Long = "file-system")]
    public bool FileSystem { get; init; }

    [Argument("Format string.", Long = "format")]
    public string? Format { get; init; }

    [Argument("Format string.", Short = 'c')]
    public string? FormatShort { get; init; }

    [Argument("Printf format string.", Long = "printf")]
    public string? Printf { get; init; }

    [Argument("Terse output.", Short = 't', Long = "terse")]
    public bool Terse { get; init; }

    [Argument("Input path.", Name = "path")]
    public string? Path { get; init; }
}
