using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class RgCommandTests
{
    [Fact]
    public void ParsesPatternPathAndOptions()
    {
        var parsed = Parser.Parse<RgArgs>(new[] { "-niS", "-g", "*.cs", "-C", "2", "--hidden", "needle", "src" });

        Assert.True(parsed.LineNumber);
        Assert.True(parsed.IgnoreCase);
        Assert.True(parsed.SmartCase);
        Assert.Equal("*.cs", parsed.Glob);
        Assert.Equal(2, parsed.Context);
        Assert.True(parsed.Hidden);
        Assert.Equal("needle", parsed.Pattern);
        Assert.Equal("src", parsed.Path);
    }

    [Fact]
    public void AcceptsSingleDashLongOptions()
    {
        var parsed = Parser.Parse<RgArgs>(new[] { "-hidden", "-no-ignore", "-files-with-matches", "-pcre2" });

        Assert.True(parsed.Hidden);
        Assert.True(parsed.NoIgnore);
        Assert.True(parsed.FilesWithMatches);
        Assert.True(parsed.Pcre2);
    }
}

[Arguments("rg command.")]
public partial record RgArgs
{
    [Argument("Shows line numbers.", Short = 'n')]
    public bool LineNumber { get; init; }

    [Argument("Ignores case.", Short = 'i')]
    public bool IgnoreCase { get; init; }

    [Argument("Uses smart case.", Short = 'S')]
    public bool SmartCase { get; init; }

    [Argument("Glob filter.", Short = 'g', Long = "glob")]
    public string? Glob { get; init; }

    [Argument("Uses fixed strings.", Short = 'F')]
    public bool FixedStrings { get; init; }

    [Argument("Matches words.", Short = 'w')]
    public bool WordRegexp { get; init; }

    [Argument("Context lines.", Short = 'C')]
    public int? Context { get; init; }

    [Argument("After context.", Short = 'A')]
    public int? AfterContext { get; init; }

    [Argument("Before context.", Short = 'B')]
    public int? BeforeContext { get; init; }

    [Argument("Maximum count.", Short = 'm')]
    public int? MaxCount { get; init; }

    [Argument("Uses PCRE2.", Short = 'P', Long = "pcre2")]
    public bool Pcre2 { get; init; }

    [Argument("No-op unrestricted flag.", Short = 'u')]
    public bool Unrestricted { get; init; }

    [Argument("Includes hidden files.", Long = "hidden")]
    public bool Hidden { get; init; }

    [Argument("Disables ignore files.", Long = "no-ignore")]
    public bool NoIgnore { get; init; }

    [Argument("Lists files.", Long = "files")]
    public bool Files { get; init; }

    [Argument("Lists files with matches.", Long = "files-with-matches")]
    public bool FilesWithMatches { get; init; }

    [Argument("Emits JSON.", Long = "json")]
    public bool Json { get; init; }

    [Argument("Pattern.", Name = "pattern")]
    public string? Pattern { get; init; }

    [Argument("Path.", Name = "path")]
    public string? Path { get; init; }
}
