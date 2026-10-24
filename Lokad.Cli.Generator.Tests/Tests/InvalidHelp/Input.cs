using Lokad.Cli;

namespace Test;

[Arguments("Nonboolean help.")]
public partial record StringHelp([property: Argument("Help.", Long = "help", IsHelp = true)] string Help);

[Arguments("Positional help flag.")]
public partial record PositionalHelp([property: Argument("Help.", IsHelp = true)] bool Help);

[Arguments("Numeric help flag.")]
public partial record NumericHelp([property: Argument("Help.", Short = 'h', ShortNumeric = true, IsHelp = true)] bool Help);

[Arguments("Collection help flag.")]
public partial record CollectionHelp([property: Argument("Help.", Long = "help", IsHelp = true)] bool[] Help);

[Arguments("Optional help value.")]
public partial record OptionalHelp([property: Argument("Help.", Long = "help", IsHelp = true)] OptionalValue<bool> Help);

[Arguments("Missing discriminator.", IsHelp = true)]
public partial record MissingHelpDiscriminator;

[Choice(typeof(HelpArityFlag))]
[Choice(typeof(HelpArityValue))]
public partial interface IHelpArity { }

[Arguments("Flag.", Prefix = "flag")]
public partial record HelpArityFlag([property: Argument("Help.", Long = "help", IsHelp = true)] bool Help) : IHelpArity;

[Arguments("Value.", Prefix = "value")]
public partial record HelpArityValue([property: Argument("Value.", Long = "help")] string Value) : IHelpArity;

[Arguments("Cyclic help.", Prefix = "help", IsHelp = true)]
[Choice(typeof(ICyclicHelp))]
public partial interface ICyclicHelp { }

[Choice(typeof(FirstHelpPrefix))]
[Choice(typeof(SecondHelpPrefix))]
public partial interface IDuplicateHelpPrefixes { }

[Arguments("Help.", Prefix = "help", IsHelp = true)]
public partial record FirstHelpPrefix : IDuplicateHelpPrefixes;

[Arguments("Other.", Prefix = "help")]
public partial record SecondHelpPrefix : IDuplicateHelpPrefixes;
