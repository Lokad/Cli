using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class SortCommandTests
{
    [Fact]
    public void ParsesFlagsSeparatorKeyAndPath()
    {
        var parsed = Parser.Parse<SortArgs>(new[] { "-unhrVf", "-t", ",", "-k", "2,3", "input" });

        Assert.True(parsed.Unique);
        Assert.True(parsed.Numeric);
        Assert.True(parsed.HumanNumeric);
        Assert.True(parsed.Reverse);
        Assert.True(parsed.VersionSort);
        Assert.True(parsed.IgnoreCase);
        Assert.Equal(",", parsed.FieldSeparator);
        Assert.Equal("2,3", parsed.Key);
        Assert.Equal("input", parsed.Path);
    }

    [Fact]
    public void RejectsLongOptions()
    {
        var ex = Assert.Throws<ParseException>(() => Parser.Parse<SortArgs>(new[] { "--unique" }));

        Assert.Equal("unknown option `--unique`", ex.Message);
    }
}

[Arguments("sort command.")]
public partial record SortArgs
{
    [Argument("Unique output.", Short = 'u')]
    public bool Unique { get; init; }

    [Argument("Numeric sort.", Short = 'n')]
    public bool Numeric { get; init; }

    [Argument("Human numeric sort.", Short = 'h')]
    public bool HumanNumeric { get; init; }

    [Argument("Reverse sort.", Short = 'r')]
    public bool Reverse { get; init; }

    [Argument("Version sort.", Short = 'V')]
    public bool VersionSort { get; init; }

    [Argument("Ignore case.", Short = 'f')]
    public bool IgnoreCase { get; init; }

    [Argument("Field separator.", Short = 't')]
    public string? FieldSeparator { get; init; }

    [Argument("Key spec.", Short = 'k')]
    public string? Key { get; init; }

    [Argument("Input path.", Name = "path")]
    public string? Path { get; init; }
}
