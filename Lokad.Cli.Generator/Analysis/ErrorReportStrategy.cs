using System;
using System.Collections.Generic;
using Lokad.Cli.Generator.Emit;

namespace Lokad.Cli.Generator.Analysis
{
    public sealed class ErrorReportStrategy : ITypeGenerationStrategy
    {
        public ErrorReportStrategy(Exception exception, CliTypeInfo type)
        {
            Type = type;
            Diagnostics = new[]
            {
                exception as GeneratorException
                ?? new GeneratorException(
                    GeneratorException.InternalError,
                    exception.Message,
                    exception.StackTrace)
            };
        }

        public CliTypeInfo Type { get; }

        public IReadOnlyList<GeneratorException> Diagnostics { get; }

        public string? EmitSource() => null;
    }
}
