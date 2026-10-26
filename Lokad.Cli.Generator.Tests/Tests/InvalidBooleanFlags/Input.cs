using Lokad.Cli;

namespace Test;

[Arguments("Positional boolean.")]
public partial record PositionalBool([property: Argument("Value.")] bool Value);

[Arguments("Nullable positional boolean.")]
public partial record NullablePositionalBool([property: Argument("Value.")] bool? Value);

[Arguments("Numeric boolean.")]
public partial record NumericBool([property: Argument("Value.", ShortNumeric = true)] bool Value);

[Arguments("Named numeric boolean.")]
public partial record NamedNumericBool([property: Argument("Value.", Long = "value", ShortNumeric = true)] bool? Value);
