using Lokad.Cli;

namespace Test;

[Arguments("Leading dash alias.")]
public partial record LeadingDashAlias
{
    [Argument("Flag.", LongNames = new[] { "flag", "--wrong" })]
    public bool Flag { get; init; }
}

[Arguments("Empty alias.")]
public partial record EmptyAlias
{
    [Argument("Flag.", LongNames = new[] { "flag", "" })]
    public bool Flag { get; init; }
}

[Arguments("Invalid short alias.")]
public partial record InvalidShortAlias
{
    [Argument("Flag.", ShortNames = new[] { 'f', '=' })]
    public bool Flag { get; init; }
}

[Arguments("Null alias array.")]
public partial record NullAliases
{
    [Argument("Flag.", LongNames = null)]
    public bool Flag { get; init; }
}

[Arguments("Long collision.")]
public partial record LongAliasCollision
{
    [Argument("Output.", Long = "output")]
    public string? Output { get; init; }

    [Argument("Target.", LongNames = new[] { "target", "output" })]
    public string? Target { get; init; }
}

[Arguments("Short collision.")]
public partial record ShortAliasCollision
{
    [Argument("First.", Short = 'f')]
    public bool First { get; init; }

    [Argument("Second.", ShortNames = new[] { 's', 'f' })]
    public bool Second { get; init; }
}

[Arguments("Primary collision.")]
public partial record PrimaryNameCollision
{
    [Argument("First.", Long = "flag")]
    public bool First { get; init; }

    [Argument("Second.", Long = "flag")]
    public bool Second { get; init; }
}

[Arguments("Discriminator argument.", Long = "run")]
public partial record DiscriminatorAlias
{
    [Argument("Run.", LongNames = new[] { "run", "execute" })]
    public bool Run { get; init; }
}

[Choice(typeof(NestedDiscriminatorAlias))]
[Arguments("Parent discriminator.", Short = 'g')]
public partial interface IParentChoice
{
}

[Arguments("Nested command.", Prefix = "nested")]
public partial record NestedDiscriminatorAlias : IParentChoice
{
    [Argument("Group.", ShortNames = new[] { 'g', 'G' })]
    public bool Group { get; init; }
}

[Arguments("Conflicting short declarations.")]
public partial record ConflictingShortNames
{
    [Argument("Flag.", Short = 'f', ShortNames = new[] { 'F' })]
    public bool Flag { get; init; }
}

[Arguments("Conflicting long declarations.")]
public partial record ConflictingLongNames
{
    [Argument("Flag.", LongNames = new[] { "flag" }, Long = "flag")]
    public bool Flag { get; init; }
}

[Arguments("Conflicting default short declaration.")]
public partial record ConflictingDefaultShortName
{
    [Argument("Flag.", ShortNames = new[] { 'f' }, Short = '\0')]
    public bool Flag { get; init; }
}

[Arguments("Conflicting default long declaration.")]
public partial record ConflictingDefaultLongName
{
    [Argument("Flag.", Long = null, LongNames = new[] { "flag" })]
    public bool Flag { get; init; }
}
