using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class DuCommandTests
{
    [Fact]
    public void ParsesShortOptionsLongOptionsAndPath()
    {
        var parsed = Parser.Parse<DuArgs>(new[] { "-aHLskx", "--bytes", "--total", "--si", "--max-depth=3", "src" });

        Assert.True(parsed.All);
        Assert.True(parsed.DereferenceCommandLine);
        Assert.True(parsed.DereferenceAll);
        Assert.True(parsed.Summarize);
        Assert.True(parsed.Kilobytes);
        Assert.True(parsed.OneFileSystem);
        Assert.True(parsed.Total);
        Assert.True(parsed.Bytes);
        Assert.True(parsed.Si);
        Assert.Equal(3, parsed.MaxDepth);
        Assert.Equal("src", parsed.Path);
    }

    [Fact]
    public void ParsesMaxDepthSeparatedValue()
    {
        var parsed = Parser.Parse<DuArgs>(new[] { "-c", "-d", "4" });

        Assert.True(parsed.Total);
        Assert.Equal(4, parsed.MaxDepth);
    }
}

[Arguments("du command.")]
public partial record DuArgs
{
    [Argument("Writes counts for all files.", Short = 'a')]
    public bool All { get; init; }

    [Argument("Follows command-line symlinks.", Short = 'H')]
    public bool DereferenceCommandLine { get; init; }

    [Argument("Follows all symlinks.", Short = 'L')]
    public bool DereferenceAll { get; init; }

    [Argument("Shows only totals.", Short = 's')]
    public bool Summarize { get; init; }

    [Argument("Uses kilobytes.", Short = 'k')]
    public bool Kilobytes { get; init; }

    [Argument("Stays on one file system.", Short = 'x')]
    public bool OneFileSystem { get; init; }

    [Argument("Shows total.", Short = 'c', Long = "total")]
    public bool Total { get; init; }

    [Argument("Uses bytes.", Long = "bytes")]
    public bool Bytes { get; init; }

    [Argument("Uses SI units.", Long = "si")]
    public bool Si { get; init; }

    [Argument("Maximum depth.", Short = 'd', Long = "max-depth")]
    public int? MaxDepth { get; init; }

    [Argument("Path operand.", Name = "path")]
    public string? Path { get; init; }
}
