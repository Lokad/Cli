# Changelog

## 1.1.0 — 2026-10-27

Published on [NuGet](https://www.nuget.org/packages/Lokad.Cli/1.1.0).

### Added

- Inspect and rewrite generated tokens with `Parser.Tokenize<T>` and
  `Parser.ParseTokenized<T>`, using the same schema as ordinary parsing.
- Detect declared help before binding required arguments or converting values
  with `Parser.IsHelpRequested<T>`.
- Share arguments across commands through reusable `[Arguments]` groups.
- Give options multiple short and long names, such as `-r`, `-R`, `--recursive`
  and `--recurse`. Allow repeated boolean flags such as `-vv` explicitly.
- Control attached short-option values, verbatim next values such as
  `--number -2`, and option boundaries for commands that pass through arguments.
- Distinguish an omitted option, a bare `--format`, and `--format=json` with
  `OptionalValue<T>`.
- Bind custom `IParsable<T>` classes and read-only collections. Nonfinal
  positional collections can stop at a conversion or option boundary.

### Fixed

- Diagnose invalid names, ambiguous choices, unsupported declarations and
  unassignable members before emitting parser code.
- Handle split partial declarations, global namespaces, escaped identifiers,
  member-name collisions and externally aliased argument types.
- Preserve command-looking option values and later operands that repeat a
  command prefix. Scope help and option names to the selected entrypoint.
- Preserve invariant built-in conversions through assembly aliases, and report
  missing numeric shorthand as `-<number>`.

### Packaging

- Bundle the current generator when build outputs are relocated, without
  publishing a separate generator package.
- Include runtime XML documentation, package symbols, the project README and
  a dedicated package icon. Write packages directly to `artifacts/nugets/`.
- Verify tests and the actual package on Windows and Linux.

### Compatibility

- Handwritten `ICliParsed<T>` implementations must now implement the static
  `Tokenize` and `IsHelpRequested` members. Annotated types receive them from
  the generator.
- Constructor-bound `required` members need `[SetsRequiredMembers]` on their
  constructor. Invalid declarations that previously generated broken code now
  produce compiler diagnostics.

## 1.0.0 — 2026-06-17

Published on [NuGet](https://www.nuget.org/packages/Lokad.Cli/1.0.0).

- Source-generated command-line parsing for .NET 8 and .NET 10.
- Typed positional arguments, named options, command choices and generated help.
- A single package containing the runtime library and source generator.
