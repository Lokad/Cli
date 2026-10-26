using Lokad.Cli;

namespace Test;

[Arguments("Mixed long discriminator.", Prefix = "run", Long = "run")]
public partial record PrefixAndLong;

[Arguments("Mixed short discriminator.", Prefix = "run", Short = 'r')]
public partial record PrefixAndShort;

[Arguments("Mixed help discriminators.", Prefix = "help", Long = "help", Short = 'h', IsHelp = true)]
public partial record MixedHelp;
