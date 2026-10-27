using Lokad.Cli;

namespace Test;

[Arguments("Ambiguous constructors.")]
public partial class Overloads
{
    public Overloads([Argument("Number.", Long = "number")] int number) { }
    public Overloads([Argument("Text.", Long = "text")] string text) { }
}

[Arguments("Incomplete constructor binding.")]
public partial class Incomplete
{
    public Incomplete([Argument("Number.", Long = "number")] int number, string text) { }
}

[Arguments("Unbound constructor.")]
public partial class Unbound
{
    public Unbound(int number) { }
}

[Arguments("Ref constructor.")]
public partial class RefConstructor
{
    public RefConstructor([Argument("Number.", Long = "number")] ref int number) { }
}

[Arguments("Ambiguous optional constructors.")]
public partial class AmbiguousOptional
{
    public AmbiguousOptional(int x = 1) { }
    public AmbiguousOptional(int x = 1, int y = 2) { }
}

[Arguments("Ambiguous params constructors.")]
public partial class AmbiguousParams
{
    public AmbiguousParams(params int[] values) { }
    public AmbiguousParams(params string[] values) { }
}

[Arguments("Ambiguous group constructor.")]
public partial record AmbiguousConstructorGroup([property: Argument("Options.")] AmbiguousOptional Options);

[Arguments("Required constructor-bound property.")]
public partial class RequiredProperty
{
    [Argument("Value.", Long = "value")] public required int Value { get; init; }
    public RequiredProperty(int Value) { this.Value = Value; }
}

[Arguments("Required constructor-bound field.")]
public partial class RequiredField
{
    [Argument("Value.", Long = "value")] public required int value;
    public RequiredField([Argument("Value.", Long = "value")] int value) { this.value = value; }
}

[Arguments("Required member in a group.")]
public partial record RequiredConstructorGroup([property: Argument("Options.")] RequiredProperty Options);
