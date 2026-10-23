using Lokad.Cli;

namespace Test;

[Arguments("Raw flag.")]
public partial record RawFlag
{
    [Argument("Flag.", Short = 'f', ConsumeNextValueVerbatim = true)] public bool Flag { get; init; }
}

[Arguments("Raw positional.")]
public partial record RawPositional
{
    [Argument("Value.", ConsumeNextValueVerbatim = true)] public string? Value { get; init; }
}

[Arguments("Raw numeric short.")]
public partial record RawNumeric
{
    [Argument("Value.", ShortNumeric = true, ConsumeNextValueVerbatim = true)] public int Value { get; init; }
}

[Arguments("Raw empty name.")]
public partial record RawEmptyName
{
    [Argument("Value.", Long = "", ConsumeNextValueVerbatim = true)] public string? Value { get; init; }
}

[Arguments("Invalid syntax.", ShortOptionSyntax = (ShortOptionSyntax)10)]
public partial record InvalidSyntax;

[Arguments("Attached choice.", ShortOptionSyntax = ShortOptionSyntax.AttachedValues)]
[Choice(typeof(FlagChoice))]
[Choice(typeof(ValueChoice))]
public partial interface IAmbiguous { }

[Arguments("Reversed attached choice.", ShortOptionSyntax = ShortOptionSyntax.AttachedValues)]
[Choice(typeof(ValueChoice))]
[Choice(typeof(FlagChoice))]
public partial interface IReversedAmbiguous { }

[Arguments("Flag.", Prefix = "flag")]
public partial record FlagChoice : IAmbiguous, IReversedAmbiguous
{
    [Argument("Value.", Short = 'x')] public bool Value { get; init; }
}

[Arguments("Value.", Prefix = "value")]
public partial record ValueChoice : IAmbiguous, IReversedAmbiguous
{
    [Argument("Value.", Short = 'x')] public string? Value { get; init; }
}

[Arguments("Raw choice.")]
[Choice(typeof(RawChoice))]
[Choice(typeof(NormalChoice))]
public partial interface IRawConflict { }

[Arguments("Raw.", Prefix = "raw")]
public partial record RawChoice : IRawConflict
{
    [Argument("Value.", Long = "value", ConsumeNextValueVerbatim = true)] public string? Value { get; init; }
}

[Arguments("Normal.", Prefix = "normal")]
public partial record NormalChoice : IRawConflict
{
    [Argument("Value.", Long = "value")] public string? Value { get; init; }
}

[Arguments("Outer.", ShortOptionSyntax = ShortOptionSyntax.AttachedValues)]
[Choice(typeof(ConflictingSyntax))]
public partial interface ISyntaxConflict { }

[Arguments("Inner.", ShortOptionSyntax = ShortOptionSyntax.GroupedValues)]
public partial record ConflictingSyntax : ISyntaxConflict;

[Arguments("Outer.", AllowSingleDashLongOptions = false)]
[Choice(typeof(ConflictingDash))]
public partial interface IDashConflict { }

[Arguments("Inner.", AllowSingleDashLongOptions = true)]
public partial record ConflictingDash : IDashConflict;
