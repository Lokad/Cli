using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Lokad.Cli.Generator
{
    public static class SymbolExtensions
    {
        public static IEnumerable<AttributeData> GetAttributes(
            this ISymbol symbol,
            Compilation compilation,
            TypeRecognizer recognizer)
        {
            foreach (var attribute in symbol.GetAttributes())
                if (attribute.AttributeClass != null && recognizer(compilation, attribute.AttributeClass))
                    yield return attribute;
        }

        public static AttributeData? TryGetAttribute(
            this ISymbol symbol,
            Compilation compilation,
            TypeRecognizer recognizer)
        =>
            symbol.GetAttributes(compilation, recognizer).FirstOrDefault();
    }
}
