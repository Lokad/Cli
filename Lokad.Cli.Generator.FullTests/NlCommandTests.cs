using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class NlCommandTests
{
    [Fact]
    public void ParsesShortAndLongOptions()
    {
        var parsed = Parser.Parse<NlArgs>(new[] { "-p", "-b", "a", "--number-width", "6", "--starting-line-number=3", "file" });

        Assert.True(parsed.NoRenumber);
        Assert.Equal("a", parsed.BodyNumbering);
        Assert.Equal("6", parsed.NumberWidth);
        Assert.Equal("3", parsed.StartingLineNumber);
        Assert.Equal("file", parsed.Path);
    }

    [Fact]
    public void RecognizesAllLongValueOptions()
    {
        var parsed = Parser.Parse<NlArgs>(new[] { "--section-delimiter=::", "--footer-numbering", "n", "--header-numbering", "t", "--line-increment", "2", "--join-blank-lines", "1", "--number-format", "rn", "--number-separator", " | " });

        Assert.Equal("::", parsed.SectionDelimiter);
        Assert.Equal("n", parsed.FooterNumbering);
        Assert.Equal("t", parsed.HeaderNumbering);
        Assert.Equal("2", parsed.LineIncrement);
        Assert.Equal("1", parsed.JoinBlankLines);
        Assert.Equal("rn", parsed.NumberFormat);
        Assert.Equal(" | ", parsed.NumberSeparator);
    }
}

[Arguments("nl command.")]
public partial record NlArgs
{
    [Argument("Body numbering.", Short = 'b', Long = "body-numbering")]
    public string? BodyNumbering { get; init; }

    [Argument("Section delimiter.", Short = 'd', Long = "section-delimiter")]
    public string? SectionDelimiter { get; init; }

    [Argument("Footer numbering.", Short = 'f', Long = "footer-numbering")]
    public string? FooterNumbering { get; init; }

    [Argument("Header numbering.", Short = 'h', Long = "header-numbering")]
    public string? HeaderNumbering { get; init; }

    [Argument("Line increment.", Short = 'i', Long = "line-increment")]
    public string? LineIncrement { get; init; }

    [Argument("Join blank lines.", Short = 'l', Long = "join-blank-lines")]
    public string? JoinBlankLines { get; init; }

    [Argument("Number format.", Short = 'n', Long = "number-format")]
    public string? NumberFormat { get; init; }

    [Argument("Does not renumber.", Short = 'p', Long = "no-renumber")]
    public bool NoRenumber { get; init; }

    [Argument("Number separator.", Short = 's', Long = "number-separator")]
    public string? NumberSeparator { get; init; }

    [Argument("Starting line number.", Short = 'v', Long = "starting-line-number")]
    public string? StartingLineNumber { get; init; }

    [Argument("Number width.", Short = 'w', Long = "number-width")]
    public string? NumberWidth { get; init; }

    [Argument("Input path.", Name = "path")]
    public string? Path { get; init; }
}
