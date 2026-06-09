namespace Lokad.Cli.Tests;

public class BindingTests
{
    [Fact]
    public void BindsOptionsAndPositionals()
    {
        var parsed = Parser.Parse<BoundArgs>(new[]
        {
            "--name", "abc",
            "-v",
            "-5",
            "42",
            "--optional=text"
        });

        Assert.Equal("abc", parsed.Name);
        Assert.True(parsed.Verbose);
        Assert.Equal(5, parsed.Depth);
        Assert.Equal(42, parsed.Count);
        Assert.Equal("text", parsed.Optional);
    }

    [Fact]
    public void ReportsMissingRequiredOption()
    {
        var ex = Assert.Throws<ParseException>(() => Parser.Parse<BoundArgs>(new[] { "42" }));
        Assert.Equal("missing option `--name`", ex.Message);
    }

    [Fact]
    public void ReportsInvalidNumericValue()
    {
        var ex = Assert.Throws<ParseException>(() => Parser.Parse<BoundArgs>(new[] { "--name", "abc", "nope" }));
        Assert.Equal("invalid value `nope` for `argument`", ex.Message);
    }

    [Fact]
    public void DispatchesChoiceByDiscriminator()
    {
        var parsed = Parser.Parse<ICommand>(new[] { "--run", "--name", "job" });
        var run = Assert.IsType<RunCommand>(parsed);
        Assert.Equal("job", run.Name);
    }

}

[Arguments("Bound argument test command.")]
public partial record BoundArgs
{
    [Argument("Name value.", Long = "name", Short = 'n')]
    public string Name { get; init; } = "";

    [Argument("Enable verbose output.", Long = "verbose", Short = 'v')]
    public bool Verbose { get; init; }

    [Argument("Numeric short depth.", ShortNumeric = true)]
    public int? Depth { get; init; }

    [Argument("Required count.")]
    public int Count { get; init; }

    [Argument("Optional text.", Long = "optional")]
    public string? Optional { get; init; }
}

[Choice(typeof(RunCommand))]
[Choice(typeof(StopCommand))]
public partial interface ICommand
{
}

[Arguments("Run command.", Long = "run")]
public partial record RunCommand : ICommand
{
    [Argument("Run name.", Long = "name")]
    public string Name { get; init; } = "";
}

[Arguments("Stop command.", Long = "stop")]
public partial record StopCommand : ICommand;
