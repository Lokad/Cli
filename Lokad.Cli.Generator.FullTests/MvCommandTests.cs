using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class MvCommandTests
{
    [Fact]
    public void ParsesNoClobberSourceAndDestination()
    {
        var parsed = Parser.Parse<MvArgs>(new[] { "-n", "source", "dest" });

        Assert.True(parsed.NoClobber);
        Assert.Equal("source", parsed.Source);
        Assert.Equal("dest", parsed.Destination);
    }

    [Fact]
    public void RequiresSourceAndDestination()
    {
        var ex = Assert.Throws<ParseException>(() => Parser.Parse<MvArgs>(new[] { "only" }));

        Assert.Equal("missing argument", ex.Message);
    }
}

[Arguments("mv command.")]
public partial record MvArgs
{
    [Argument("Enables no-clobber behavior.", Short = 'n')]
    public bool NoClobber { get; init; }

    [Argument("Source operand.", Name = "source")]
    public string Source { get; init; } = "";

    [Argument("Destination operand.", Name = "destination")]
    public string Destination { get; init; } = "";
}
