using Lokad.Cli;

namespace Test;

[Arguments("Ranges command.")]
public partial record RangeOptions
{
    [Argument("Line ranges.", Long = "ranges", Short = 'r')]
    public Ranges Ranges { get; init; } = new(System.Array.Empty<(int, int)>());
}
