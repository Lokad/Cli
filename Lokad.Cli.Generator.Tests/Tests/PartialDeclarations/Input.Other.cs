using Lokad.Cli;

namespace Test;

[Arguments("Split arguments.")]
public partial record SplitArgs;

[Choice(typeof(SecondChoice))]
public partial interface ICommand { }

[Arguments("Second.", Prefix = "second")]
public partial record SecondChoice : ICommand;
