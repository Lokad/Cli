using Lokad.Cli;

namespace Test;

[Arguments(Long = "mode", Short = 'm')]
public partial record TokenizationOptions(
    [property: Argument(Long = "from-ctor", Short = 'c')]
    string FromConstructor,
    [Argument(Long = "raw-param", Short = 'p')]
    string RawParameter)
{
    [Argument(Long = "verbose", Short = 'v')]
    public bool Verbose { get; init; }

    [Argument(ShortNumeric = true)]
    public int? Depth { get; init; }

    [Argument(Long = "field", Short = 'f')]
    public string? Field;
}
