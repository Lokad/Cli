using System.Collections.Generic;
using Lokad.Cli.Generator.Emit;

namespace Lokad.Cli.Generator.Analysis
{
    public sealed class ParserGenerationStrategy : ITypeGenerationStrategy
    {
        public ParserGenerationStrategy(CliTypeInfo type)
        {
            Type = type;
        }

        public CliTypeInfo Type { get; }

        public IReadOnlyList<GeneratorException> Diagnostics => EmptyDiagnostics.Value;

        public bool WillEmitSource => true;

        public string? EmitSource() => EmitParserType.EmitFor(Type);

        public bool WillEmitImplementation => false;

        public string? EmitImplementation() => null;

        private static class EmptyDiagnostics
        {
            public static readonly IReadOnlyList<GeneratorException> Value = new GeneratorException[0];
        }
    }
}
