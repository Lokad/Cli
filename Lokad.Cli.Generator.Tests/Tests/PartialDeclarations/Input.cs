using Lokad.Cli;

namespace Test;

public partial record SplitArgs
{
    [Argument("Value.", Long = "value")] public int Value { get; init; }
}

[Choice(typeof(FirstChoice))]
public partial interface ICommand { }

[Arguments("First.", Prefix = "first")]
public partial record FirstChoice : ICommand;
