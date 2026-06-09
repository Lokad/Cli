using System;
using Microsoft.CodeAnalysis;

namespace Lokad.Cli.Generator.Analysis
{
    public static class TypeSymbolRecognizer
    {
        internal static TypeRecognizer IsNamed(string fullyQualifiedName)
        {
            var comparer = SymbolEqualityComparer.Default;
            var weakCompilation = new WeakReference<Compilation?>(null);
            var weakType = new WeakReference<ITypeSymbol?>(null);

            return (compilation, type) =>
            {
                if (weakCompilation.TryGetTarget(out var cachedCompilation) &&
                    weakType.TryGetTarget(out var cachedType) &&
                    ReferenceEquals(cachedCompilation, compilation))
                {
                    return comparer.Equals(cachedType, type);
                }

                var maybeType = compilation.GetTypeByMetadataName(fullyQualifiedName);
                if (maybeType != null)
                {
                    if (comparer.Equals(maybeType, type))
                    {
                        weakCompilation.SetTarget(compilation);
                        weakType.SetTarget(maybeType);
                        return true;
                    }
                }

                if (fullyQualifiedName == type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ||
                    fullyQualifiedName == type.ToDisplayString())
                {
                    weakCompilation.SetTarget(compilation);
                    weakType.SetTarget(type);
                    return true;
                }

                return false;
            };
        }
    }
}
