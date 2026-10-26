using System.Collections.Generic;
using Lokad.Cli.Generator.Emit;

namespace Lokad.Cli.Generator.Analysis
{
    public sealed class ParserGenerationStrategy : ITypeGenerationStrategy
    {
        private readonly CliTypeInfo _type;

        public ParserGenerationStrategy(CliTypeInfo type)
        {
            _type = type;
        }

        public TypeName TypeName => _type.TypeName;

        public IReadOnlyList<GeneratorException> Diagnostics => System.Array.Empty<GeneratorException>();

        public string EmitSource() => EmitParserType.EmitFor(_type);
    }
}
