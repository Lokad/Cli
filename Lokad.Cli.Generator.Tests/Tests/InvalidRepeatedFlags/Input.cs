using System;
using System.Collections.Generic;
using Lokad.Cli;

namespace Test;

[Arguments("Non-boolean argument.")]
public partial record NonBooleanRepeatedFlag
{
    [Argument("Count.", Long = "count", AllowRepeated = true)]
    public int Count { get; init; }
}

[Arguments("Positional argument.")]
public partial record PositionalRepeatedFlag
{
    [Argument("Flag.", AllowRepeated = true)]
    public bool Flag { get; init; }
}

[Arguments("Collection argument.")]
public partial record CollectionRepeatedFlag
{
    [Argument("Flags.", Long = "flags", AllowRepeated = true)]
    public bool[] Flags { get; init; } = [];
}

[Arguments("Numeric short argument.")]
public partial record NumericRepeatedFlag
{
    [Argument("Flag.", Short = 'f', ShortNumeric = true, AllowRepeated = true)]
    public bool Flag { get; init; }
}

[Arguments("Empty long option name.")]
public partial record EmptyLongRepeatedFlag
{
    [Argument("Flag.", Long = "", AllowRepeated = true)]
    public bool Flag { get; init; }
}

[Arguments("List argument.")]
public partial record ListRepeatedFlag
{
    [Argument("Flags.", Long = "flags", AllowRepeated = true)]
    public IReadOnlyList<bool> Flags { get; init; } = Array.Empty<bool>();
}

[Arguments("Memory argument.")]
public partial record MemoryRepeatedFlag
{
    [Argument("Flags.", Long = "flags", AllowRepeated = true)]
    public ReadOnlyMemory<bool> Flags { get; init; }
}
