using Lokad.Cli;

namespace Test;

[Arguments("Getter only.")]
public partial record GetterOnly
{
    [Argument("Value.", Long = "value")] public int Value { get; } = 42;
}

[Arguments("Readonly field.")]
public partial record ReadonlyField
{
    [Argument("Value.", Long = "value")] public readonly int Value = 42;
}

[Arguments("Static field.")]
public partial record StaticField
{
    [Argument("Value.", Long = "value")] public static int Value;
}

[Arguments("Static property.")]
public partial record StaticProperty
{
    [Argument("Value.", Long = "value")] public static int Value { get; set; }
}

[Arguments("Constant field.")]
public partial record ConstantField
{
    [Argument("Value.", Long = "value")] public const int Value = 42;
}

[Arguments("Indexer.")]
public partial record IndexedProperty
{
    [Argument("Value.", Long = "value")] public int this[int index] { get => 42; set { } }
}
