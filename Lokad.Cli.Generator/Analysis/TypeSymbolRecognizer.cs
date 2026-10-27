using System;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace Lokad.Cli.Generator.Analysis
{
    public static class TypeSymbolRecognizer
    {
        internal static TypeRecognizer FromResolver(Func<Compilation, INamedTypeSymbol?> resolve)
        {
            var types = new ConditionalWeakTable<Compilation, ResolvedType>();
            ConditionalWeakTable<Compilation, ResolvedType>.CreateValueCallback create =
                compilation => new ResolvedType(resolve(compilation));

            return (compilation, type) => SymbolEqualityComparer.Default.Equals(types.GetValue(compilation, create).Symbol, type);
        }

        private sealed class ResolvedType
        {
            public ResolvedType(INamedTypeSymbol? symbol) => Symbol = symbol;

            public INamedTypeSymbol? Symbol { get; }
        }
    }
}
