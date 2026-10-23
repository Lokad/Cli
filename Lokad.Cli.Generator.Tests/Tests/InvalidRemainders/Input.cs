using Lokad.Cli;

namespace Test;

[Arguments("Scalar remainder.")]
public partial record ScalarRemainder([property: Argument("Tail.", Remainder = true)] string Tail);

[Arguments("Typed remainder.")]
public partial record TypedRemainder([property: Argument("Tail.", Remainder = true)] int[] Tail);

[Arguments("Named remainder.")]
public partial record NamedRemainder([property: Argument("Tail.", Long = "tail", Remainder = true)] string[] Tail);

[Arguments("Nonfinal remainder.")]
public partial record NonfinalRemainder(
    [property: Argument("Tail.", Remainder = true)] string[] Tail,
    [property: Argument("File.")] string File);

[Arguments("Collection selector.")]
public partial record CollectionSelector(
    [property: Argument("Selectors.")] string[] Selectors,
    [property: Argument("Tail.", Remainder = true)] string[] Tail);

[Arguments("Missing boundary.")]
public partial record MissingBoundary([property: Argument("Tail.", Remainder = true)] string[] Tail);

[Arguments("Flag boundary.")]
public partial record FlagBoundary([property: Argument("Flag.", Long = "flag", StopOptionParsing = true)] bool Flag);

[Arguments("Collection option boundary.")]
public partial record CollectionBoundary([property: Argument("Code.", Long = "code", StopOptionParsing = true)] string[] Code);

[Arguments("Optional option boundary.")]
public partial record OptionalBoundary([property: Argument("Code.", Long = "code", StopOptionParsing = true)] OptionalValue<string> Code);

[Arguments("Positional boundary.")]
public partial record PositionalBoundary([property: Argument("Code.", StopOptionParsing = true)] string Code);

[Arguments("Grouped short boundary.")]
public partial record GroupedBoundary([property: Argument("Code.", Short = 'c', StopOptionParsing = true)] string Code);

[Arguments("Conflicting policy.", StopAtFirstOperand = true)]
[Choice(typeof(ConflictingOperandPolicy))]
public partial interface IOperandPolicy { }

[Arguments("Conflicting policy.", StopAtFirstOperand = false)]
public partial record ConflictingOperandPolicy : IOperandPolicy;

[Arguments("Duplicate selector.", StopAtFirstOperand = true)]
[Choice(typeof(DuplicateFirst))]
[Choice(typeof(DuplicateSecond))]
public partial interface IDuplicate { }

[Arguments("First.", Prefix = "run")]
public partial record DuplicateFirst : IDuplicate;

[Arguments("Second.", Prefix = "run")]
public partial record DuplicateSecond : IDuplicate;

[Arguments("Multiple defaults.", StopAtFirstOperand = true)]
[Choice(typeof(DefaultFirst))]
[Choice(typeof(DefaultSecond))]
public partial interface IDefaults { }

[Arguments("First.")]
public partial record DefaultFirst : IDefaults;

[Arguments("Second.")]
public partial record DefaultSecond : IDefaults;

[Arguments("Cycle.", StopAtFirstOperand = true)]
[Choice(typeof(ICycle))]
public partial interface ICycle { }

[Arguments("Default nested choice.", StopAtFirstOperand = true)]
[Choice(typeof(INestedDefault))]
public partial interface IDefaultRoot { }

[Choice(typeof(NestedDefaultLeaf))]
public partial interface INestedDefault : IDefaultRoot { }

[Arguments("Leaf.", Prefix = "run")]
public partial record NestedDefaultLeaf : INestedDefault;
