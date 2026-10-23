using Lokad.Cli;

namespace Test;

[Arguments("Scalar boundary.")]
public partial record ScalarBoundary
{
    [Argument("Number.", ConsumeUntilBoundary = true)] public int Number { get; init; }
    [Argument("Label.")] public string Label { get; init; } = "";
}

[Arguments("Named boundary.")]
public partial record NamedBoundary
{
    [Argument("Numbers.", Long = "number", ConsumeUntilBoundary = true)] public int[] Numbers { get; init; } = [];
    [Argument("Label.")] public string Label { get; init; } = "";
}

[Arguments("Final boundary.")]
public partial record FinalBoundary
{
    [Argument("Numbers.", ConsumeUntilBoundary = true)] public int[] Numbers { get; init; } = [];
}

[Arguments("Numeric short boundary.")]
public partial record NumericShortBoundary
{
    [Argument("Numbers.", ShortNumeric = true, ConsumeUntilBoundary = true)] public int[] Numbers { get; init; } = [];
    [Argument("Label.")] public string Label { get; init; } = "";
}

[Arguments("Adjacent boundary.")]
public partial record AdjacentBoundary
{
    [Argument("Numbers.", ConsumeUntilBoundary = true)] public int[] Numbers { get; init; } = [];
    [Argument("Verbose.", Long = "verbose")] public bool Verbose { get; init; }
    [Argument("Labels.")] public string[] Labels { get; init; } = [];
}
