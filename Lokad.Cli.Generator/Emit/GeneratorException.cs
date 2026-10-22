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
    }
}
