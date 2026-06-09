using Lokad.Cli;

namespace Test;

[Choice(typeof(AddCommand))]
[Choice(typeof(RemoveCommand))]
[Choice(typeof(ListCommand))]
public partial interface ICommand
{
}

[Arguments(Long = "add")]
public partial record AddCommand : ICommand
{
    [Argument(Long = "name")]
    public string? Name { get; init; }
}

[Arguments(Short = 'r')]
public partial record RemoveCommand : ICommand
{
    [Argument(Short = 'i')]
    public int? Id { get; init; }
}

[Arguments(Prefix = "list")]
public partial record ListCommand : ICommand;
