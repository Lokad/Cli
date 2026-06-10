using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class RmCommandTests
{
    [Fact]
    public void ParsesRecursiveForceAndPath()
    {
        var parsed = Parser.Parse<RmArgs>(new[] { "-rRf", "a" });

        Assert.True(parsed.RecursiveLower);
        Assert.True(parsed.RecursiveUpper);
        Assert.True(parsed.Force);
        Assert.Equal("a", parsed.Path);
    }

    [Fact]
    public void RejectsLongOptions()
    {
        var ex = Assert.Throws<ParseException>(() => Parser.Parse<RmArgs>(new[] { "--recursive", "a" }));

        Assert.Equal("unknown option `--recursive`", ex.Message);
    }

    [Fact]
    public void RequiresPathOperand()
    {
        var ex = Assert.Throws<ParseException>(() => Parser.Parse<RmArgs>(new[] { "-f" }));

        Assert.Equal("missing argument", ex.Message);
    }
}

[Arguments("rm command.")]
public partial record RmArgs
{
    [Argument("Deletes recursively.", Short = 'r')]
    public bool RecursiveLower { get; init; }

    [Argument("Deletes recursively.", Short = 'R')]
    public bool RecursiveUpper { get; init; }

    [Argument("Suppresses missing-path errors.", Short = 'f')]
    public bool Force { get; init; }

    [Argument("Path operand.", Name = "path")]
    public string Path { get; init; } = "";
}
