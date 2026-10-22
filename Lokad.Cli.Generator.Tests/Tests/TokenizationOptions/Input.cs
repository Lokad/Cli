using Lokad.Cli;

namespace Test;

[Arguments("Tokenization mode.", Long = "mode", Short = 'm')]
public partial record TokenizationOptions(
    [property: Argument("Constructor property value.", Long = "from-ctor", Short = 'c')]
    string FromConstructor,
    [Argument("Raw constructor parameter.", Long = "raw-param", Short = 'p')]
    string RawParameter)
{
    [Argument("Enable verbose output.", Long = "verbose", Short = 'v', AllowRepeated = true)]
    public bool Verbose { get; init; }

    [Argument("Numeric depth.", ShortNumeric = true)]
    public int? Depth { get; init; }

    [Argument("Field value.", Long = "field", Short = 'f')]
    public string? Field;
}
