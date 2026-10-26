# Lokad.Cli

A command-line argument parsing library based on source code generation.

Library `Lokad.Cli` contains the annotations and helper functions used at runtime.
Library `Lokad.Cli.Generator` contains the source generator.

The calling code should annotate types representing the command-line argument structure.

In case of parsing error, a `Lokad.Cli.ParseException` is thrown, and should be caught to report a proper error message.

## Basic usage

Use top-level, non-generic partial types. Unsupported declarations produce `LKC0027`.
Share CLI members through groups; inherited CLI annotations are diagnosed.
The `Parser` facade supports argument members named `Parse`, `Tokenize`, `ParseTokenized`, or `Document`.

The `[Arguments]` annotation is added to a record to indicate that it should be automatically parsed from the command-line arguments.
It requires user-facing documentation for the argument shape.

The `[Argument]` annotation is added to a property to configure how it is parsed, in conjunction with information from the argument type.
It requires user-facing documentation for the argument.
For positional arguments, set `Name` to control the placeholder shown in generated documentation.

```csharp
[Arguments("Runs the application.")]
public partial record MyArgs
{
    [Argument("Input file to process.", Name = "input")]
    public string Input { get; init; } = "";

    // Type 'int?' means an optional argument that must be parsed as an integer. 
    // The presence of 'Short' and/or 'Long' means this is a named option.
    // Recognizes `-d 10`, `-d10`, `--max-depth 10` or `--max-depth=10`
    // Invalid values such as `-d abc` produce ParseException.
    [Argument(
        "Maximum depth for recursive traversal.",
        Short = 'd',
        Long  = "max-depth")]
    public int? MaxDepth { get; init; }
}
``` 

Documentation can be generated from the same metadata:

```csharp
var documentation = Lokad.Cli.Parser.Document<MyArgs>("appname");
```

This produces line-oriented documentation that starts with a usage line such as `appname <input>`.
Optional positional arguments are shown as `<input?>`; multiple-arity positional arguments are shown as `<input...>`.

The parsing is performed based on a list of string arguments: 

```csharp
public int Main(string[] args) 
{
    try
    {
        var parsed = Lokad.Cli.Parser.Parse<MyArgs>(args);
        return Operate(parsed);
    }
    catch (Lokad.Cli.ParseException ex)
    {
        Console.Error.WriteLine($"appname: {ex.Message}");
        return 1;
    }
}
``` 

### Tokenization and token rewriting

`Parser.Tokenize<T>(IReadOnlyList<string>)` uses the same generated option names and lexical rules as `Parser.Parse<T>`, without binding values or resolving choices:

```csharp
var tokens = Lokad.Cli.Parser.Tokenize<MyArgs>(new[] { "input.txt", "--max-depth=10" });
var hasDepthOption = tokens.HasLongOption("max-depth") || tokens.HasShortOption('d');
var parsed = Lokad.Cli.Parser.ParseTokenized<MyArgs>(tokens);
```

Each call returns a fresh mutable cursor. Presence checks do not advance it; `Shift` and `ParseTokenized` consume it. Unknown option names still produce `ParseException`, but missing arguments, invalid converted values, duplicate flags, and choice conflicts are checked during binding or discrimination.

Callers can rewrite token records and pass a new `TokenizedArguments` to `ParseTokenized<T>`. Preserve option-value boundaries: the tokens do not expose option arity. Reconstructing raw argument strings and tokenizing them again can change their interpretation, so normalize tokens directly. Reusable option names remain private to the generated type.

Handwritten implementations of `ICliParsed<T>` must implement its static `Tokenize` and `IsHelpRequested(TokenizedArguments)` members; annotated types receive them from the generator.

### Help inspection

Mark a named `bool`/`bool?` flag with `[Argument(..., IsHelp = true)]`, or a
help choice with `[Arguments(..., IsHelp = true)]`.
`Parser.IsHelpRequested<MyArgs>(args)` detects help before binding. Lexical errors
take precedence; help-looking text in values or remainders stays data.

The tokenized overload preserves the cursor for `Parser.ParseTokenized<MyArgs>`.
Ordinary parsing remains strict.

### Repeated boolean flags

Boolean flags reject duplicate occurrences by default. Set `AllowRepeated = true` on a named `bool` or `bool?` argument to accept repetition as a single occurrence:

```csharp
[Argument("Enable verbose output.", Short = 'v', Long = "verbose", AllowRepeated = true)]
public bool Verbose { get; init; }
```

For this declaration, `-vv`, `-v -v`, `--verbose --verbose`, and `-v --verbose` all set `Verbose` to `true`. A nullable flag remains `null` when absent. Repetition does not count occurrences or choose between different flags. Explicit values such as `--verbose=false` are still rejected; option values and operands after `--` retain their existing interpretation.

`AllowRepeated` applies to properties, fields, and constructor parameters. Using it on a value-taking argument, a positional argument, a collection, or an argument with `ShortNumeric` produces generator diagnostic `LKC0003`.

### Option aliases

Use `ShortNames` and `LongNames` to give an argument multiple spellings:

```csharp
[Argument("Recursive traversal.", ShortNames = new[] { 'r', 'R' },
    LongNames = new[] { "recursive", "recurse" }, AllowRepeated = true)]
public bool Recursive { get; init; }
```

Here `-r`, `-R`, `--recursive`, and `--recurse` share one value and repetition policy; help lists them together. Choose `Short` or `ShortNames`, and independently `Long` or `LongNames`. Combining either pair produces a compiler diagnostic.

For aliased arguments, binding errors use the first long name, or the first short name if none exists. Invalid names and collisions within one command are diagnosed; aliases on option discriminators are currently unsupported.

### Option-value syntax

These controls are independent opt-ins:

| Declaration | Behavior |
| --- | --- |
| `[Arguments(..., ShortOptionSyntax = ShortOptionSyntax.AttachedValues)]` | `-fepattern` sets flag `f` and binds `e` to `pattern`; `-ef` binds `e` to `f`, even when `f` is declared. |
| `[Argument(..., ConsumeNextValueVerbatim = true)]` | A named value-taking option accepts the next raw argument: `--number -2`, `-e --help`, or even `-e --`. |
| `[Arguments(..., AllowSingleDashLongOptions = false)]` | One dash selects short options; `--long` remains available. |

Defaults preserve grouped values such as `-ee first second`. Explicit shape settings must agree across reachable choices and their annotated interfaces; unspecified settings inherit them. Value-aware modes require unambiguous option arities across those choices. Consumed operands become attached-value tokens before choice discrimination.

Use `OptionalValue<T>` for a named option whose value is optional:

```csharp
[Argument("Output format.", Long = "format", Name = "style")]
public OptionalValue<string> Format { get; init; }
```

Omission is absent; `--format` is present without a value; `--format=json` supplies `"json"`. Check `IsPresent` and `HasValue`, then read `Value` or use `TryGetValue`. Explicit values use the existing scalar conversions. A bare occurrence never consumes the next argument; `--format=` supplies empty text. Short names require `ShortOptionSyntax.AttachedValues`: `-F` is bare, `-Fjson` has a value. Scalar repeats keep the last occurrence; collections retain every bare or valued occurrence in order. Existing `T?` options still require an operand when present.

### Literal remainders

Use `[Arguments(..., StopAtFirstOperand = true)]` to forward the suffix in
`app -v file --child-option`. Command prefixes and option values do not count as
operands. Options remain interspersed by default.

For an explicit boundary, mark a final positional string collection with
`Remainder = true`: parsing stops after its preceding scalar operands.
Alternatively, `StopOptionParsing = true` on a named scalar option forwards
everything after its required value, as in `app -c code --child-option`.
Short option boundaries require `ShortOptionSyntax.AttachedValues`.

The suffix becomes `LiteralArg` tokens, excluded from option and command
inspection; strings such as `--help` and `--` remain data. A leading `--` still
acts as the normal terminator and can start a declared remainder. Select the
command before its operands or option boundary. Ambiguous selectors and
variable-length collections before a declared remainder are diagnosed.
A remainder without preceding operands needs an option boundary or
`StopAtFirstOperand`.

### Choices and Discriminators

A discriminator can be requested as part of the `[Arguments]` itself:

 - `[Arguments("...", Prefix = "abc")]` expects an unmatched positional argument equal to `abc` 
 - `[Arguments("...", Long = "def")]` expects `--def` (without arguments) to appear as a long argument.
 - `[Arguments("...", Short = 'g')]` expects `-g` (without arguments) to appear as a short argument.
 
Being used as a discriminator does not prevent the option from appearing on an argument as well.

The `[Choice]` annotation is added to an interface that can represent multiple ways of parsing arguments. 
The choices must have a discriminant (at most one choice without discriminant is allowed).

```csharp
[Choice(typeof(MyArgs1))]
[Choice(typeof(MyArgs2))]
public partial interface IMyArgs
{
}

[Arguments("First command.", Prefix = "first")]
public partial record MyArgs1 : IMyArgs;

[Arguments("Second command.", Prefix = "second")]
public partial record MyArgs2 : IMyArgs;
```

The members of the interface are ignored. All the choice types must implement the interface, and
must carry either `[Arguments]` or `[Choice]` annotations of their own.
Option values are excluded from prefix matching; positional choices require unambiguous option arities.

The interface may be annotated with `[Arguments]` itself to specify a discriminator. For example, 

```csharp
[Choice(typeof(IMySubArgs))]
[Choice(typeof(MyArgs))]
public partial interface IMyArgs {}

[Choice(typeof(MySubArgs1))]
[Choice(typeof(MySubArgs2))]
[Arguments("Subcommands.", Prefix = "sub")]
public partial interface IMySubArgs : IMyArgs {}

// Produced by parsing `A`
[Arguments("Command A.", Prefix = "A")] public partial record MyArgs : IMyArgs;
 
// Produced by parsing `sub B` 
[Arguments("Command B.", Prefix = "B")] public partial record MySubArgs1 : IMySubArgs;

// Produced by parsing `sub C`
[Arguments("Command C.", Prefix = "C")] public partial record MySubArgs2 : IMySubArgs;
``` 

### Parsing algorithm steps

1. **Tokenization** classifies options and operands using the reachable schema
   and its lexical policies. Value-aware modes distinguish consumed values from
   command prefixes; `--` ends options unless consumed as a verbatim value.
2. **Discrimination** selects a declared choice by its option or positional prefix.
   Conflicting choices fail; a default is used only when no explicit choice matches.
3. **Binding** converts values, checks required arguments and flag repetition,
   and constructs the selected command and its groups. Invalid input produces
   `ParseException`; exceptions from custom converters propagate unchanged.

## Supported Types

The `bool` type is special; a `bool` option must have a `Short` or `Long` label. It is true if present, false if not.

The primitive types are `string`, `int`, `uint`, `long`, `ulong`, `DateTime`, `double`, all enums, `Ranges`, and any type that implements `IParsable`. A property of a primitive type will parse its argument from the string value (or take the string value as-is, for a `string`), and report an error if parsing fails: "invalid value `value`" or "invalid value `value` for `--foo`). Conversely, if there is no value available for the property, it reports "missing argument" or "missing option `--foo`" 

Custom `IParsable<T>` structs and classes are converted through `TryParse` with a `null` format provider. Returning `false` produces a `ParseException` containing the invalid value and its argument label. Exceptions thrown by the custom `TryParse` implementation propagate unchanged.

The `Ranges` type is an `IReadOnlyList<(int First, int Last)>` parsed from comma-separated range elements such as `1,4,5-6`. Each element can be a single number `N`, a closed range `N-N`, an open lower-bound range `-N` where `First` is `int.MinValue`, or an open upper-bound range `N-` where `Last` is `int.MaxValue`.

Given a primitive type `T`, the type `T?` behaves the same, but the property is optional and does not fail if no value was provided. 

For a supported element type `T`, `T[]`, `IReadOnlyList<T>`, and `ReadOnlyMemory<T>` capture multiple values. Repeated named options append in order (`--value 10 --value 11` or `-vv 10 11`); a final positional collection captures the remaining operands. Omitted collections are empty. Elements use the same conversions as scalar values; boolean collections take explicit `true` or `false` values.

A nonfinal positional collection captures one value by default. Set `ConsumeUntilBoundary = true` on its `[Argument]` to collect until conversion fails or an option follows its first element. The next positional member receives the unconverted value. For `int[] numbers`, `string label`, and `int count`, input `10 11 label 12` then binds `[10, 11]`, `"label"`, and `12`. No trailing values are reserved: a string collection needs an option boundary to pass values to later members. This mode requires a nonfinal positional collection followed by a scalar; invalid declarations produce `LKC0008` or `LKC0009`.

### Reusable option groups

Embed an `[Arguments]` type as an unnamed `[Argument]` to share its members:

```csharp
[Arguments("Common options.")]
public partial record CommonOptions(
    [property: Argument("Timeout.", Long = "timeout")] int Timeout,
    [property: Argument("Verbose.", Short = 'v')] bool Verbose);

[Arguments("Copy.")]
public partial record CopyArgs(
    [property: Argument("Input.")] string Input,
    [property: Argument("Common options.")] CommonOptions Common);
```

`app input.txt --timeout 30 -v` fills `Input` and constructs `Common`.
Nested groups are supported; their leaves bind and appear in documentation at
that position, using the same parsing, help and tokenization rules as flat members.
Explicit lexical settings on commands, groups and their annotated interfaces
must agree.

Groups must be non-nullable, non-collection concrete types without choices or
discriminators. Their constructors and assigned members must be accessible from
the containing command. Invalid group uses and cycles produce `LKC0024`;
conflicting option names produce `LKC0005` with member paths.
