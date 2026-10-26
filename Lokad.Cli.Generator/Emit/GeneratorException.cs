using System;
using Microsoft.CodeAnalysis;

namespace Lokad.Cli.Generator.Emit
{
    public sealed class GeneratorException : Exception
    {
        public GeneratorException(DiagnosticDescriptor descriptor, params object?[] messageArgs)
            : base(descriptor.Title.ToString())
        {
            Descriptor = descriptor;
            MessageArgs = messageArgs;
        }

        public GeneratorException(Location? location, DiagnosticDescriptor descriptor, params object?[] messageArgs)
            : base(descriptor.Title.ToString())
        {
            Location = location;
            Descriptor = descriptor;
            MessageArgs = messageArgs;
        }

        public DiagnosticDescriptor Descriptor { get; }

        public object?[] MessageArgs { get; }

        public Location? Location { get; }

#pragma warning disable RS2008

        public static readonly DiagnosticDescriptor InternalError = new DiagnosticDescriptor(
            id: "LKC0001",
            title: "Internal error",
            messageFormat: "Source generator exception '{0}' at stack trace {1}",
            category: "SourceGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor InvalidRepeatedFlag = new DiagnosticDescriptor(
            id: "LKC0003",
            title: "Invalid repeated flag",
            messageFormat: "Argument '{0}' uses AllowRepeated, which requires a named bool or bool? flag without ShortNumeric",
            category: "SourceGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor InvalidOptionName = new DiagnosticDescriptor(
            id: "LKC0004",
            title: "Invalid option name",
            messageFormat: "Invalid {0}: option names must be nonempty, cannot start with a dash, and cannot contain '=', whitespace or control characters",
            category: "SourceGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor OptionNameCollision = new DiagnosticDescriptor(
            id: "LKC0005",
            title: "Option name collision",
            messageFormat: "Option '{0}' is assigned to both '{1}' and '{2}' in the same command",
            category: "SourceGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor DiscriminatorAliases = new DiagnosticDescriptor(
            id: "LKC0006",
            title: "Unsupported discriminator aliases",
            messageFormat: "Argument '{0}' has aliases and shares '{1}' with a reachable option discriminator; discriminator aliases are not supported",
            category: "SourceGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor ConflictingOptionNames = new DiagnosticDescriptor(
            id: "LKC0007",
            title: "Conflicting option name declarations",
            messageFormat: "Use either {0} or {1} on an argument, not both",
            category: "SourceGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor InvalidCollectionBoundary = new DiagnosticDescriptor(
            id: "LKC0008",
            title: "Invalid positional collection boundary",
            messageFormat: "Argument '{0}' uses ConsumeUntilBoundary, which requires a nonfinal positional collection",
            category: "SourceGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor AdjacentCollectionBoundary = new DiagnosticDescriptor(
            id: "LKC0009",
            title: "Unsupported adjacent positional collections",
            messageFormat: "Argument '{0}' uses ConsumeUntilBoundary immediately before collection '{1}'; adjacent positional collections are not supported in this mode",
            category: "SourceGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor InvalidVerbatimValue = new DiagnosticDescriptor(
            id: "LKC0010",
            title: "Invalid verbatim option value",
            messageFormat: "Argument '{0}' uses ConsumeNextValueVerbatim, which requires a named value-taking option without ShortNumeric",
            category: "SourceGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor AmbiguousOptionArity = new DiagnosticDescriptor(
            id: "LKC0011",
            title: "Ambiguous option value mode",
            messageFormat: "Option '{0}' has conflicting value modes across reachable choices; value-aware tokenization requires an unambiguous mode",
            category: "SourceGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor ConflictingTokenizationPolicy = new DiagnosticDescriptor(
            id: "LKC0012",
            title: "Conflicting tokenization policy",
            messageFormat: "Explicit '{0}' settings must agree across the reachable choice graph",
            category: "SourceGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor InvalidTokenizationPolicy = new DiagnosticDescriptor(
            id: "LKC0013",
            title: "Invalid tokenization policy",
            messageFormat: "Invalid value for '{0}'",
            category: "SourceGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor InvalidOptionalValue = new DiagnosticDescriptor(
            id: "LKC0014",
            title: "Invalid optional option value",
            messageFormat: "Argument '{0}' uses OptionalValue<T>, which requires a named option without a nullable wrapper, nested OptionalValue<T>, ShortNumeric, AllowRepeated or ConsumeNextValueVerbatim",
            category: "SourceGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor OptionalShortSyntax = new DiagnosticDescriptor(
            id: "LKC0015",
            title: "Optional short values require attached syntax",
            messageFormat: "Short options using OptionalValue<T> require ShortOptionSyntax.AttachedValues on the argument shape or a reachable choice interface",
            category: "SourceGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor InvalidRemainder = new DiagnosticDescriptor(
            "LKC0016", "Invalid literal remainder",
            "Argument '{0}' uses Remainder, which requires a positional string collection without ConsumeUntilBoundary",
            "SourceGenerator", DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor InvalidStopOption = new DiagnosticDescriptor(
            "LKC0017", "Invalid option boundary",
            "Argument '{0}' uses StopOptionParsing, which requires a named scalar option with a required value",
            "SourceGenerator", DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor InvalidRemainderLayout = new DiagnosticDescriptor(
            "LKC0018", "Invalid remainder layout",
            "Remainder '{0}' must be the final positional argument, follow only scalars, and have a preceding operand, a StopOptionParsing option or StopAtFirstOperand",
            "SourceGenerator", DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor AmbiguousCommandBoundary = new DiagnosticDescriptor(
            "LKC0019", "Ambiguous command boundary",
            "Literal boundaries require unambiguous command selectors: {0}",
            "SourceGenerator", DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor ShortBoundarySyntax = new DiagnosticDescriptor(
            "LKC0020", "Unsupported short option boundary",
            "Short options using StopOptionParsing require ShortOptionSyntax.AttachedValues",
            "SourceGenerator", DiagnosticSeverity.Error, true);
        public static readonly DiagnosticDescriptor InvalidHelpFlag = new DiagnosticDescriptor(
            "LKC0021", "Invalid help flag",
            "Argument '{0}' uses IsHelp, which requires a named bool or bool? flag without ShortNumeric",
            "SourceGenerator", DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor InvalidHelpChoice = new DiagnosticDescriptor(
            "LKC0022", "Missing help discriminator",
            "An argument shape using IsHelp requires a Prefix, Long or Short discriminator",
            "SourceGenerator", DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor AmbiguousHelpSchema = new DiagnosticDescriptor(
            "LKC0023", "Ambiguous help command schema",
            "Help prefix inspection requires unambiguous command selectors: {0}",
            "SourceGenerator", DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor InvalidArgumentGroup = new DiagnosticDescriptor(
            "LKC0024", "Invalid argument group",
            "Argument group '{0}' is invalid: {1}",
            "SourceGenerator", DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor InvalidBindingTarget = new DiagnosticDescriptor(
            "LKC0025", "Invalid argument binding target",
            "Argument '{0}' requires a constructor parameter, writable instance field or writable non-indexed instance property",
            "SourceGenerator", DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor InvalidChoiceSchema = new DiagnosticDescriptor(
            "LKC0026", "Invalid command choices",
            "Command choices require an acyclic graph and unambiguous selectors: {0}",
            "SourceGenerator", DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor UnsupportedArgumentType = new DiagnosticDescriptor(
            "LKC0027", "Unsupported argument type declaration",
            "CLI argument type '{0}' is unsupported: {1}",
            "SourceGenerator", DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor InvalidArgumentConstructor = new DiagnosticDescriptor(
            "LKC0028", "Invalid argument constructor",
            "CLI argument type '{0}' has an invalid constructor: {1}",
            "SourceGenerator", DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor InvalidBooleanFlag = new DiagnosticDescriptor(
            "LKC0029", "Invalid boolean flag",
            "Boolean argument '{0}' requires a named option without ShortNumeric",
            "SourceGenerator", DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor NumericShortCollision = new DiagnosticDescriptor(
            "LKC0030", "Ambiguous numeric short option",
            "Short option '-{0}' cannot be used alongside ShortNumeric arguments in the reachable command graph",
            "SourceGenerator", DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor InvalidChoiceTarget = new DiagnosticDescriptor(
            "LKC0031", "Invalid command choice target",
            "Choice '{0}' on '{1}' is invalid: {2}",
            "SourceGenerator", DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor InvalidPositionalOrder = new DiagnosticDescriptor(
            "LKC0032", "Required positional argument after optional argument",
            "Optional positional argument '{0}' cannot precede required positional argument '{1}'",
            "SourceGenerator", DiagnosticSeverity.Error, true);
    }
}
