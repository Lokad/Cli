using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class NumericOptionCommandTests
{
    [Fact]
    public void DocumentsNumericShorthandWithSingularNamedOptions()
    {
        Assert.Contains("-n, -<number>: Line count.", Parser.Document<HeadArgs>("head"));
        Assert.Contains("--depth, -<number>: Depth.", Parser.Document<NamedNumericArgs>("app"));
    }

    [Fact]
    public void AllowsNumericShorthandInMutuallyExclusiveCommands()
    {
        Assert.Equal(7, Assert.IsType<NumericFirst>(Parser.Parse<INumericChoice>(new[] { "first", "-7" })).Number);
        Assert.Equal(9, Assert.IsType<NumericSecond>(Parser.Parse<INumericChoice>(new[] { "second", "-9" })).Number);
    }
}

[Arguments("Named numeric shorthand.")]
public partial record NamedNumericArgs([property: Argument("Depth.", Long = "depth", ShortNumeric = true)] int? Depth);

[Choice(typeof(NumericFirst))]
[Choice(typeof(NumericSecond))]
public partial interface INumericChoice { }

[Arguments("First.", Prefix = "first")]
public partial record NumericFirst([property: Argument("Number.", ShortNumeric = true)] int? Number) : INumericChoice;

[Arguments("Second.", Prefix = "second")]
public partial record NumericSecond([property: Argument("Number.", ShortNumeric = true)] int? Number) : INumericChoice;
