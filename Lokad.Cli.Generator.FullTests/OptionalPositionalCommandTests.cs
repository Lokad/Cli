using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class OptionalPositionalCommandTests
{
    [Fact]
    public void PreservesTrailingOptionalsAndRequiredNamedOptions()
    {
        var omitted = Parser.Parse<TrailingOptionalArgs>(new[] { "report.txt" });
        Assert.Equal("report.txt", omitted.File);
        Assert.Null(omitted.Limit);
        Assert.Empty(omitted.Tail);
        var supplied = Parser.Parse<TrailingOptionalArgs>(new[] { "report.txt", "7", "tail" });
        Assert.Equal(7, supplied.Limit);
        Assert.Equal(new[] { "tail" }, supplied.Tail);
        var named = Parser.Parse<OptionalBeforeNamedArgs>(new[] { "--file=report.txt" });
        Assert.Null(named.Limit);
        Assert.Equal("report.txt", named.File);
    }
}

[Arguments("Trailing optional operands.")]
public partial record TrailingOptionalArgs(
    [property: Argument("File.")] string File,
    [property: Argument("Limit.")] int? Limit,
    [property: Argument("Tail.")] string[] Tail);

[Arguments("Optional operand before a named option.")]
public partial record OptionalBeforeNamedArgs(
    [property: Argument("Limit.")] int? Limit,
    [property: Argument("File.", Long = "file")] string File);
