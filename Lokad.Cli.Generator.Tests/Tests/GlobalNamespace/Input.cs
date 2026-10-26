using Lokad.Cli;

[Arguments("Global arguments.")]
public partial record GlobalArgs([property: Argument("Value.", Long = "value")] int Value);
