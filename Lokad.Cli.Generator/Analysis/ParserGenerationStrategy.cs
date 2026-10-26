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

        public IReadOnlyList<GeneratorException> Diagnostics => System.Array.Empty<GeneratorException>();

        public string EmitSource() => EmitParserType.EmitFor(Type);
    }
}
