# Lokad.Cli

A command-line argument parsing library based on source code generation.

Library `Lokad.Cli` contains the annotations and helper functions used at runtime.
Library `Lokad.Cli.Generator` contains the source generator.

The calling code should annotate types representing the command-line argument structure.

In case of parsing error, a `Lokad.Cli.ParseException` is thrown, and should be caught to report a proper error message.

## Basic usage

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
    // Recognizes `-d abc` and reports "Invalid integer value for option `-d`: abc"
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
        MyArgs args = Lokad.Cli.Parser.Parse<MyArgs>(args);
        return Operate(args);
    }
    catch (Lokad.Cli.ParseException ex)
    {
        Console.Error.WriteLine($"appname: {ex.Message}");
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

Handwritten implementations of `ICliParsed<T>` must implement its static `Tokenize` member; annotated types receive it from the generator.

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
```

The members of the interface are ignored. All the choice types must implement the interface, and
must carry either `[Arguments]` or `[Choice]` annotations of their own.

The interface may be annotated with `[Arguments]` itself to specify a discriminator. For example, 

```csharp
[Choice(typeof(IMySubArgs))]
[Choice(typeof(MyArgs)]
public partial interface IMyArgs {}

[Choice(typeof(MySubArgs1)]
[Choice(typeof(MySubArgs2)]
[Arguments("Subcommands.", Prefix = "sub")]
public partial interface IMySubArgs : IMyArgs {}

// Produced by parsing `A`
[Arguments("Command A.", Prefix = "A"] public partial record MyArgs();
 
// Produced by parsing `sub B` 
[Arguments("Command B.", Prefix = "B"] public partial record MySubArgs1();

// Produced by parsing `sub C`
[Arguments("Command C.", Prefix = "C"] public partial record MySubArgs2();
``` 

### Parsing algorithm steps

The parsing algorithm works as follows: 

  1. **Tokenization**: run left-to-right using reachable option names and, for the opt-in value syntax above, generated value modes. The following rules describe the default syntax.
     - if an `[Argument("...", ShortNumeric=true)]` exists, `-[0-9]+` is treated as `NumericShortArg`
     - if an `[Argument("...", ShortNumeric=true, Short='x')]` exists, `-x[0-9]+` is treated as `NumericShortArg`
     - `-[a-zA-Z0-9]+` can be a `LongArg` if `[Argument("...", Long="xyz")]` exists, otherwise, it becomes a `ShortArgSet` made up of single-character short arguments. Reports "unknown short option -x" for each character for which `[Argument("...", Short='x')]` does not exist.
     - `--[a-zA-Z0-9]+` is a `LongArg`. Reports "unknown option `--xyz`" if `[Argument("...", Long="xyz")]` does not exist.
     - `--[a-zA-Z0-9]+=.*` is a `LongArgWithValue`. Reports "unknown option `--xyz`" if `[Argument("...", Long="xyz")]` does not exist.
     - `--` is ignored, but causes all subsequent arguments to be treated as `ValueArg`.
     - Any unmatched argument is treated a `ValueArg`.
     
  2. **Discrimination**: if multiple `[Choice]` are present, find which one to use based on discriminants. 
     - If multiple choices match, report an error "incompatible options `--abc` and `--def`".
     - If all choices have a discriminant, and none of them match, report an error "expected `--abc`, `--def` or `--ghi`"
     
  3. **Binding**: after choices have resolved, traverse the argument list and divert each argument to its destination. 
     - Options without arguments (usually, booleans) are assigned.
     - Options that include their argument value (`NumericShortArg`, `LongArgWithValue`) have their value parsed and assigned.
     - Options that expect a subsequent value (a `LongArg` or an entry in a `ShortArgSet`) consume the next unparsed argument ; if there is no next unparsed argument or if that argument is not a `ValueArg`, report an error "option `--foo` expects a value" ; the consumed argument is parsed and assigned.
     - Options that are not allowed (and are not a discriminator), are reported as "unexpected option `--xyz`" (as opposed to "unknown option")
     - Any `ValueArg`, if not consumed by a preceding option, is parsed assigned to the next positional property (this includes the positional discriminator). If there are no unbound positional properties remaining, report "unexpected argument `{value}`"

## Supported Types

The `bool` type is special; a `bool` option must have a `Short` or `Long` label. It is true if present, false if not.

The primitive types are `string`, `int`, `uint`, `long`, `ulong`, `DateTime`, `double`, all enums, `Ranges`, and any type that implements `IParsable`. A property of a primitive type will parse its argument from the string value (or take the string value as-is, for a `string`), and report an error if parsing fails: "invalid value `value`" or "invalid value `value` for `--foo`). Conversely, if there is no value available for the property, it reports "missing argument" or "missing option `--foo`" 

Custom `IParsable<T>` structs and classes are converted through `TryParse` with a `null` format provider. Returning `false` produces a `ParseException` containing the invalid value and its argument label. Exceptions thrown by the custom `TryParse` implementation propagate unchanged.

The `Ranges` type is an `IReadOnlyList<(int First, int Last)>` parsed from comma-separated range elements such as `1,4,5-6`. Each element can be a single number `N`, a closed range `N-N`, an open lower-bound range `-N` where `First` is `int.MinValue`, or an open upper-bound range `N-` where `Last` is `int.MaxValue`.

Given a primitive type `T`, the type `T?` behaves the same, but the property is optional and does not fail if no value was provided. 

For a supported element type `T`, `T[]`, `IReadOnlyList<T>`, and `ReadOnlyMemory<T>` capture multiple values. Repeated named options append in order (`--value 10 --value 11` or `-vv 10 11`); a final positional collection captures the remaining operands. Omitted collections are empty. Elements use the same conversions as scalar values; boolean collections take explicit `true` or `false` values.

A nonfinal positional collection captures one value by default. Set `ConsumeUntilBoundary = true` on its `[Argument]` to collect until conversion fails or an option follows its first element. The next positional member receives the unconverted value. For `int[] numbers`, `string label`, and `int count`, input `10 11 label 12` then binds `[10, 11]`, `"label"`, and `12`. No trailing values are reserved: a string collection needs an option boundary to pass values to later members. This mode requires a nonfinal positional collection followed by a scalar; invalid declarations produce `LKC0008` or `LKC0009`.

### Reusable option groups
  
There is a special behavior for types that are annotated with `[Arguments]`:
  - the type must be used as a positional property
  - the type must be used as-is (no `?` or collection)
  - the type must not contain a positional discriminator

A C# compiler error is reported by the source generator if a type annotated with `[Arguments]` is used in a way that does not satisfy these two constraints. 

In preparation for the binding phase, the members of the inner type are inserted into the outer type as if they had been present at that position. This means that: 

```csharp
[Arguments("Inner arguments.")]
public partial record Inner
{
  [Argument("A value.")] public int A { get; init }
  [Argument("B flag.", Short="b")] public bool B { get; init; }
}

[Arguments("Outer arguments.")]
public partial record Outer
{
  [Argument("C value.")] public string C { get; init; }
  [Argument("Inner group.")] public Inner I { get; init; }
  [Argument("D value.", Short="d")] public string? D { get; init; }
}
```

Will follow exactly the same parsing rules as: 


```csharp
[Arguments("Outer arguments.")]
public partial record Outer
{
  [Argument("C value.")] public string C { get; init; }
  [Argument("A value.")] public int A { get; init }
  [Argument("B flag.", Short="b")] public bool B { get; init; }
  [Argument("D value.", Short="d")] public string? D { get; init; }
}
```





     
       
