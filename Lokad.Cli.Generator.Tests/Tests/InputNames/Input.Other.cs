using Lokad.Cli;

public partial record InputArgs
{
    [Argument("Value.", Long = "value")] public int Value { get; init; }
}
