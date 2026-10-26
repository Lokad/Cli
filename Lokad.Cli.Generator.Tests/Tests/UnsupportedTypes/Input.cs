using Lokad.Cli;

namespace Test;

[Arguments("Generic.")]
public partial record GenericArgs<T>([property: Argument("Value.")] T Value);

public class Host
{
    [Arguments("Nested.")]
    public partial record NestedArgs([property: Argument("Value.")] int Value);
}

[Arguments("Ref-like.")]
public ref partial struct RefArgs { }

[Arguments("Non-partial.")]
public record NonPartialArgs;

[Arguments("File-local.")]
file partial record FileArgs;
