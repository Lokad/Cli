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

        public GeneratorException(SourceLocationInfo? location, DiagnosticDescriptor descriptor, params object?[] messageArgs)
            : base(descriptor.Title.ToString())
        {
            Location = location;
            Descriptor = descriptor;
            MessageArgs = messageArgs;
        }

        public DiagnosticDescriptor Descriptor { get; }

        public object?[] MessageArgs { get; }

        public SourceLocationInfo? Location { get; }

#pragma warning disable RS2008

        public static readonly DiagnosticDescriptor InternalError = new DiagnosticDescriptor(
            id: "LKC0001",
            title: "Internal error",
            messageFormat: "Source generator exception '{0}' at stack trace {1}",
            category: "SourceGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor NotImplemented = new DiagnosticDescriptor(
            id: "LKC0002",
            title: "Generator not implemented",
            messageFormat: "Lokad.Cli source generation is not implemented yet for type '{0}'",
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
            messageFormat: "Invalid {0}: use an array of nonempty names without leading dashes, '=', whitespace or control characters",
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
            messageFormat: "Option '{0}' has conflicting value modes across reachable choices; attached short values and verbatim operands require an unambiguous mode",
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
    }
}
