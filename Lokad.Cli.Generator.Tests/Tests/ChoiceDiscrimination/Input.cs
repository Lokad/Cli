using Lokad.Cli;

namespace Test;

[Choice(typeof(AddCommand))]
[Choice(typeof(RemoveCommand))]
[Choice(typeof(ListCommand))]
public partial interface ICommand
{
}

[Arguments("Add command.", Long = "add")]
public partial record AddCommand : ICommand
{
    [Argument("Name to add.", Long = "name")]
    public string? Name { get; init; }
}

[Arguments("Remove command.", Short = 'r')]
public partial record RemoveCommand : ICommand
{
    [Argument("Identifier to remove.", Short = 'i')]
    public int? Id { get; init; }
}

[Arguments("List command.", Prefix = "list")]
public partial record ListCommand : ICommand;
