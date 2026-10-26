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

    [Fact]
    public void UsesConstructorsCallableWithoutArgumentsForCommandsAndGroups()
    {
        var paramsArgs = Parser.Parse<ParamsConstructorArgs>(new[] { "--value=7" });
        Assert.Equal(7, paramsArgs.Value);
        Assert.Equal(0, paramsArgs.ExtraCount);
        var optionalArgs = Parser.Parse<OptionalConstructorArgs>(new[] { "--value=8" });
        Assert.Equal(8, optionalArgs.Value);
        Assert.True(optionalArgs.Initialized);
        Assert.Equal(9, Parser.Parse<ParamsConstructorGroup>(new[] { "--value=9" }).Options.Value);
        Assert.True(Parser.Parse<OptionalConstructorGroup>(new[] { "--value=10" }).Options.Initialized);
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

[Arguments("Params constructor.")]
public partial class ParamsConstructorArgs
{
    public ParamsConstructorArgs(params string[] extras) => ExtraCount = extras.Length;
    public int ExtraCount { get; }
    [Argument("Value.", Long = "value")] public int Value { get; init; }
}

[Arguments("Optional consumer constructor.")]
public partial class OptionalConstructorArgs
{
    public OptionalConstructorArgs(bool initialize = true) => Initialized = initialize;
    public bool Initialized { get; }
    [Argument("Value.", Long = "value")] public int Value { get; init; }
}

[Arguments("Params group.")]
public partial record ParamsConstructorGroup([property: Argument("Options.")] ParamsConstructorArgs Options);

[Arguments("Optional constructor group.")]
public partial record OptionalConstructorGroup([property: Argument("Options.")] OptionalConstructorArgs Options);
