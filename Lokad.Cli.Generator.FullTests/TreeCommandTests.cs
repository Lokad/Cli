using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class TreeCommandTests
{
    [Fact]
    public void ParsesDepthIgnorePatternAndFlags()
    {
        var parsed = Parser.Parse<TreeArgs>(new[] { "-a", "-d", "-f", "-h", "-C", "-L", "2", "-I", "bin|obj", "--dirsfirst", "src" });

        Assert.True(parsed.All);
        Assert.True(parsed.DirectoriesOnly);
        Assert.True(parsed.FullPath);
        Assert.True(parsed.HumanReadable);
        Assert.True(parsed.Color);
        Assert.Equal(2, parsed.Depth);
        Assert.Equal("bin|obj", parsed.IgnorePattern);
        Assert.True(parsed.DirsFirst);
        Assert.Equal("src", parsed.Path);
    }
}

[Arguments("tree command.")]
public partial record TreeArgs
{
    [Argument("Depth.", Short = 'L')]
    public int? Depth { get; init; }

    [Argument("Shows all files.", Short = 'a')]
    public bool All { get; init; }

    [Argument("Directories only.", Short = 'd')]
    public bool DirectoriesOnly { get; init; }

    [Argument("Full path.", Short = 'f')]
    public bool FullPath { get; init; }

    [Argument("Ignore patterns.", Short = 'I')]
    public string? IgnorePattern { get; init; }

    [Argument("Human-readable sizes.", Short = 'h')]
    public bool HumanReadable { get; init; }

    [Argument("Color output.", Short = 'C')]
    public bool Color { get; init; }

    [Argument("Directories first.", Long = "dirsfirst")]
    public bool DirsFirst { get; init; }

    [Argument("Input path.", Name = "path")]
    public string? Path { get; init; }
}
