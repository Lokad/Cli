using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class BindingTargetCommandTests
{
    [Fact]
    public void ConstructsReadOnlyMembersThroughAnnotatedParameters()
    {
        Assert.Equal(7, Parser.Parse<ConstructorReadOnlyArgs>(new[] { "--value=7" }).Value);
    }
}

[Arguments("Read-only result.")]
public partial class ConstructorReadOnlyArgs
{
    public ConstructorReadOnlyArgs([Argument("Value.", Long = "value")] int value) => Value = value;
    public int Value { get; }
}
