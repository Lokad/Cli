using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class GrepCommandTests
{
    [Fact]
    public void ParsesShortOptionsLongOptionsPatternAndPath()
    {
        var parsed = Parser.Parse<GrepArgs>(new[] { "-nri", "--invert-match", "--max-count", "2", "--glob=*.cs", "needle", "src" });

        Assert.True(parsed.LineNumber);
        Assert.True(parsed.Recursive);
        Assert.True(parsed.IgnoreCase);
        Assert.True(parsed.InvertMatch);
        Assert.Equal(2, parsed.MaxCount);
        Assert.Equal("*.cs", parsed.Glob);
        Assert.Equal("needle", parsed.Pattern);
        Assert.Equal("src", parsed.Path);
    }

    [Fact]
    public void RecognizesAllListedFlags()
    {
        var parsed = Parser.Parse<GrepArgs>(new[] { "-RvI", "-m", "1", "--files-with-matches", "--with-filename", "--word-regexp", "--fixed-strings", "--binary-files=without-match", "--extended-regexp", "--hidden", "--no-ignore", "--files", "--pcre2", "--json" });

        Assert.True(parsed.RecursiveUpper);
        Assert.True(parsed.InvertMatch);
        Assert.True(parsed.FilesWithMatches);
        Assert.True(parsed.WithFilename);
        Assert.True(parsed.WordRegexp);
        Assert.True(parsed.FixedStrings);
        Assert.True(parsed.IgnoreBinaryFiles);
        Assert.True(parsed.ExtendedRegexp);
        Assert.True(parsed.BinaryFilesWithoutMatch);
        Assert.True(parsed.Hidden);
        Assert.True(parsed.NoIgnore);
        Assert.True(parsed.Files);
        Assert.True(parsed.Pcre2);
        Assert.True(parsed.Json);
        Assert.Equal(1, parsed.MaxCount);
    }
}

[Arguments("grep command.")]
public partial record GrepArgs
{
    [Argument("Shows line numbers.", Short = 'n', Long = "line-number")]
    public bool LineNumber { get; init; }

    [Argument("Searches recursively.", Short = 'r', Long = "recursive")]
    public bool Recursive { get; init; }

    [Argument("Searches recursively.", Short = 'R')]
    public bool RecursiveUpper { get; init; }

    [Argument("Ignores case.", Short = 'i', Long = "ignore-case")]
    public bool IgnoreCase { get; init; }

    [Argument("Inverts matches.", Short = 'v', Long = "invert-match")]
    public bool InvertMatch { get; init; }

    [Argument("Lists matching files.", Short = 'l', Long = "files-with-matches")]
    public bool FilesWithMatches { get; init; }

    [Argument("Shows filenames.", Short = 'H', Long = "with-filename")]
    public bool WithFilename { get; init; }

    [Argument("Matches whole words.", Short = 'w', Long = "word-regexp")]
    public bool WordRegexp { get; init; }

    [Argument("Uses fixed strings.", Short = 'F', Long = "fixed-strings")]
    public bool FixedStrings { get; init; }

    [Argument("Ignores binary files.", Short = 'I')]
    public bool IgnoreBinaryFiles { get; init; }

    [Argument("Uses extended regex.", Short = 'E', Long = "extended-regexp")]
    public bool ExtendedRegexp { get; init; }

    [Argument("Binary files mode.", Long = "binary-files")]
    public string? BinaryFiles { get; init; }

    public bool BinaryFilesWithoutMatch => BinaryFiles == "without-match";

    [Argument("Maximum match count.", Short = 'm', Long = "max-count")]
    public int? MaxCount { get; init; }

    [Argument("Glob filter.", Long = "glob")]
    public string? Glob { get; init; }

    [Argument("Includes hidden files.", Long = "hidden")]
    public bool Hidden { get; init; }

    [Argument("Disables ignore files.", Long = "no-ignore")]
    public bool NoIgnore { get; init; }

    [Argument("Lists files.", Long = "files")]
    public bool Files { get; init; }

    [Argument("Uses PCRE2.", Long = "pcre2")]
    public bool Pcre2 { get; init; }

    [Argument("Emits JSON.", Long = "json")]
    public bool Json { get; init; }

    [Argument("Pattern.", Name = "pattern")]
    public string? Pattern { get; init; }

    [Argument("Path.", Name = "path")]
    public string? Path { get; init; }
}
