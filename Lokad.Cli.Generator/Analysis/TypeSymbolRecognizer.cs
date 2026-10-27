using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace Lokad.Cli.Generator.Analysis
{
    public static class TypeSymbolRecognizer
    {
        internal static TypeRecognizer IsNamed(string fullyQualifiedName)
        {
            var types = new ConditionalWeakTable<Compilation, ResolvedType>();
            ConditionalWeakTable<Compilation, ResolvedType>.CreateValueCallback resolve =
                compilation => new ResolvedType(compilation.GetTypeByMetadataName(fullyQualifiedName));

            return (compilation, type) => SymbolEqualityComparer.Default.Equals(types.GetValue(compilation, resolve).Symbol, type);
        }

        private sealed class ResolvedType
        {
            public ResolvedType(INamedTypeSymbol? symbol) => Symbol = symbol;

            public INamedTypeSymbol? Symbol { get; }
        }
    }
}
