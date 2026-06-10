using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class CpCommandTests
{
    [Fact]
    public void ParsesRecursiveOptionsAndSourceDestination()
    {
        var parsed = Parser.Parse<CpArgs>(new[] { "-rR", "source", "destination" });

        Assert.True(parsed.RecursiveLower);
        Assert.True(parsed.RecursiveUpper);
        Assert.Equal("source", parsed.Source);
        Assert.Equal("destination", parsed.Destination);
    }

    [Fact]
    public void RejectsLongOptions()
    {
        var ex = Assert.Throws<ParseException>(() => Parser.Parse<CpArgs>(new[] { "--recursive", "source", "destination" }));

        Assert.Equal("unknown option `--recursive`", ex.Message);
    }

    [Fact]
    public void RequiresSourceAndDestination()
    {
        var ex = Assert.Throws<ParseException>(() => Parser.Parse<CpArgs>(new[] { "source" }));

        Assert.Equal("missing argument", ex.Message);
    }
}

[Arguments("cp command.")]
public partial record CpArgs
{
    [Argument("Recursively copies directories.", Short = 'r')]
    public bool RecursiveLower { get; init; }

    [Argument("Recursively copies directories.", Short = 'R')]
    public bool RecursiveUpper { get; init; }

    [Argument("Source operand.", Name = "source")]
    public string Source { get; init; } = "";

    [Argument("Destination operand.", Name = "destination")]
    public string Destination { get; init; } = "";
}
