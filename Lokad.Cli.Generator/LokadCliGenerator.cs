using Lokad.Cli.Generator.Analysis;
using Lokad.Cli.Generator.Emit;
using Microsoft.CodeAnalysis;

namespace Lokad.Cli.Generator
{
    [Generator]
    public sealed class LokadCliGenerator : BaseIncrementalGenerator
    {
        private readonly TypeRecognizer _argumentsAttribute =
            Recognize.Type("Lokad.Cli.ArgumentsAttribute");

        private readonly TypeRecognizer _choiceAttribute =
            Recognize.Type("Lokad.Cli.ChoiceAttribute");

        public LokadCliGenerator()
            : base("lkc")
        {
        }

        protected override ITypeGenerationStrategy? GetGenerationStrategyForSymbol(
            Compilation compilation,
            ITypeSymbol typeSymbol,
            CliTypeInfo type)
        {
            if (!HasCliAnnotation(compilation, typeSymbol))
                return null;

            return new ParserGenerationStrategy(type);
        }

        private bool HasCliAnnotation(Compilation compilation, ITypeSymbol typeSymbol) =>
            typeSymbol.TryGetAttribute(compilation, _argumentsAttribute) != null ||
            typeSymbol.TryGetAttribute(compilation, _choiceAttribute) != null;
    }
}
