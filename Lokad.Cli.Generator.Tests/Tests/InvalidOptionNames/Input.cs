using Lokad.Cli;

namespace Test;

[Arguments("Equals short name.")]
public partial record EqualsShort([property: Argument("Flag.", Short = '=')] bool Flag);

[Arguments("Dash short name.")]
public partial record DashShort([property: Argument("Flag.", Short = '-')] bool Flag);

[Arguments("Whitespace short name.")]
public partial record WhitespaceShort([property: Argument("Flag.", Short = ' ')] bool Flag);

[Arguments("Control short name.")]
public partial record ControlShort([property: Argument("Flag.", Short = '\n')] bool Flag);

[Arguments("Empty long name.")]
public partial record EmptyLong([property: Argument("Flag.", Long = "")] bool Flag);

[Arguments("Equals long name.")]
public partial record EqualsLong([property: Argument("Flag.", Long = "value=")] bool Flag);

[Arguments("Dash long name.")]
public partial record DashLong([property: Argument("Flag.", Long = "-flag")] bool Flag);

[Arguments("Whitespace long name.")]
public partial record WhitespaceLong([property: Argument("Flag.", Long = "two words")] bool Flag);

[Arguments("Invalid discriminator.", Short = '=')]
public partial record InvalidDiscriminator;
