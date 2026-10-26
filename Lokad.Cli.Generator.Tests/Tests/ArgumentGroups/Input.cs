using Lokad.Cli;

namespace Test;

[Arguments("Common options.")]
public partial record CommonOptions(
    [property: Argument("Timeout.", Long = "timeout")] int Timeout,
    [property: Argument("Verbose.", Short = 'v')] bool Verbose);

[Arguments("Copy.")]
public partial record CopyArgs(
    [property: Argument("Input.")] string Input,
    [property: Argument("Common options.")] CommonOptions Common);
