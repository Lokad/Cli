using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class BindingTargetCommandTests
{
    [Fact]
    public void ConstructsReadOnlyMembersThroughAnnotatedParameters()
    {
        Assert.Equal(7, Parser.Parse<ConstructorReadOnlyArgs>(new[] { "--value=7" }).Value);
    }

    [Fact]
    public void IgnoresRecordCopyConstructorForAnOriginalProperty()
    {
        Assert.Equal("input.txt", Parser.Parse<OriginalArgs>(new[] { "--source=input.txt" }).original);
        Assert.Equal("input.txt", Parser.Parse<OriginalGroupArgs>(new[] { "--source=input.txt" }).Options.original);
    }
}

[Arguments("Read-only result.")]
public partial class ConstructorReadOnlyArgs
{
    public ConstructorReadOnlyArgs() => Value = -1;
    public ConstructorReadOnlyArgs([Argument("Value.", Long = "value")] int value) => Value = value;
    public int Value { get; }
}

[Arguments("Original input.")]
public partial record OriginalArgs([property: Argument("Source.", Long = "source")] string original);

[Arguments("Grouped original input.")]
public partial record OriginalGroupArgs([property: Argument("Options.")] OriginalArgs Options);
