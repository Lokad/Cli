using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class SedCommandTests
{
    [Fact]
    public void ParsesOptionsScriptAndPath()
    {
        var parsed = Parser.Parse<SedArgs>(new[] { "-nE", "-e", "s/a/b/", "-i", "input" });

        Assert.True(parsed.SuppressAutomaticPrint);
        Assert.True(parsed.ExtendedRegex);
        Assert.True(parsed.InPlace);
        Assert.Equal("s/a/b/", parsed.Script);
        Assert.Equal("input", parsed.Path);
    }

    [Fact]
    public void RejectsLongOptions()
    {
        var ex = Assert.Throws<ParseException>(() => Parser.Parse<SedArgs>(new[] { "--quiet" }));

        Assert.Equal("unknown option `--quiet`", ex.Message);
    }
}

[Arguments("sed command.")]
public partial record SedArgs
{
    [Argument("Suppresses automatic printing.", Short = 'n')]
    public bool SuppressAutomaticPrint { get; init; }

    [Argument("Script expression.", Short = 'e')]
    public string? Script { get; init; }

    [Argument("Enables in-place editing.", Short = 'i')]
    public bool InPlace { get; init; }

    [Argument("Uses extended regex.", Short = 'E')]
    public bool ExtendedRegex { get; init; }

    [Argument("Input path.", Name = "path")]
    public string? Path { get; init; }
}
