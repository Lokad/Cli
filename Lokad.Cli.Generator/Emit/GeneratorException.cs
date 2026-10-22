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
    }
}
