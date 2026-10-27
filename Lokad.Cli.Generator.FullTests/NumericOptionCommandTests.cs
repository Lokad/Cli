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

    [Fact]
    public void ReportsMissingNumericShorthandWithItsDocumentedLabel()
    {
        var error = Assert.Throws<ParseException>(() => Parser.Parse<RequiredNumericArgs>(System.Array.Empty<string>()));
        Assert.Equal("missing option `-<number>`", error.Message);
        Assert.Contains("-<number>: Count.", Parser.Document<RequiredNumericArgs>("app"));
        Assert.Equal(7, Parser.Parse<RequiredNumericArgs>(new[] { "-7" }).Count);
    }

    [Fact]
    public void PreservesThePrimaryNameForRequiredNamedNumericOptions()
    {
        var error = Assert.Throws<ParseException>(() => Parser.Parse<RequiredNamedNumericArgs>(System.Array.Empty<string>()));
        Assert.Equal("missing option `--count`", error.Message);
        Assert.Equal(7, Parser.Parse<RequiredNamedNumericArgs>(new[] { "-7" }).Count);
        Assert.Equal(8, Parser.Parse<RequiredNamedNumericArgs>(new[] { "--limit=8" }).Count);
    }
}

[Arguments("Named numeric shorthand.")]
public partial record NamedNumericArgs([property: Argument("Depth.", Long = "depth", ShortNumeric = true)] int? Depth);

[Arguments("Required numeric shorthand.")]
public partial record RequiredNumericArgs([property: Argument("Count.", ShortNumeric = true)] int Count);

[Arguments("Required named numeric shorthand.")]
public partial record RequiredNamedNumericArgs(
    [property: Argument("Count.", LongNames = new[] { "count", "limit" }, ShortNumeric = true)] int Count);

[Choice(typeof(NumericFirst))]
[Choice(typeof(NumericSecond))]
public partial interface INumericChoice { }

[Arguments("First.", Prefix = "first")]
public partial record NumericFirst([property: Argument("Number.", ShortNumeric = true)] int? Number) : INumericChoice;

[Arguments("Second.", Prefix = "second")]
public partial record NumericSecond([property: Argument("Number.", ShortNumeric = true)] int? Number) : INumericChoice;
