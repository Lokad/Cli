using Lokad.Cli;

namespace Test;

[Arguments("Positional optional value.")]
public partial record Positional
{
    [Argument("Value.")] public OptionalValue<int> Value { get; init; }
}

[Arguments("Nullable wrapper.")]
public partial record NullableWrapper
{
    [Argument("Value.", Long = "value")] public OptionalValue<int>? Value { get; init; }
}

[Arguments("Nested wrapper.")]
public partial record NestedWrapper
{
    [Argument("Value.", Long = "value")] public OptionalValue<OptionalValue<int>> Value { get; init; }
}

[Arguments("Nullable collection element.")]
public partial record NullableElement
{
    [Argument("Values.", Long = "value")] public OptionalValue<int>?[] Values { get; init; } = [];
}

[Arguments("Numeric short.")]
public partial record NumericShort
{
    [Argument("Value.", Long = "value", ShortNumeric = true)] public OptionalValue<int> Value { get; init; }
}

[Arguments("Flag repetition.")]
public partial record RepeatedFlag
{
    [Argument("Value.", Long = "value", AllowRepeated = true)] public OptionalValue<int> Value { get; init; }
}

[Arguments("Raw following operand.")]
public partial record Verbatim
{
    [Argument("Value.", Long = "value", ConsumeNextValueVerbatim = true)] public OptionalValue<int> Value { get; init; }
}

[Arguments("Short syntax.")]
public partial record ShortSyntax
{
    [Argument("Value.", Short = 'v')] public OptionalValue<int> Value { get; init; }
}

[Arguments("Ambiguous modes.", ShortOptionSyntax = ShortOptionSyntax.AttachedValues)]
[Choice(typeof(OptionalMode))]
[Choice(typeof(RequiredMode))]
public partial interface IModeConflict { }

[Arguments("Optional mode.", Prefix = "optional")]
public partial record OptionalMode : IModeConflict
{
    [Argument("Value.", Long = "value")] public OptionalValue<string> Value { get; init; }
}

[Arguments("Required mode.", Prefix = "required")]
public partial record RequiredMode : IModeConflict
{
    [Argument("Value.", Long = "value")] public string? Value { get; init; }
}
