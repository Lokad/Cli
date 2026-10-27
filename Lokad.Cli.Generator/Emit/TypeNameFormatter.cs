using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Lokad.Cli.Generator.Emit
{
    public sealed class TypeNameFormatter
    {
        private static readonly SymbolDisplayFormat DisplayFormat =
            SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
                SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions |
                SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

        private readonly Compilation _compilation;
        private readonly Dictionary<IAssemblySymbol, string> _qualifiers =
            new Dictionary<IAssemblySymbol, string>(SymbolEqualityComparer.Default);
        private readonly SortedSet<string> _externAliases = new SortedSet<string>(StringComparer.Ordinal);

        public TypeNameFormatter(Compilation compilation) => _compilation = compilation;

        public IReadOnlyCollection<string> ExternAliases => _externAliases;

        public TypeName FromSymbol(ITypeSymbol symbol) => new TypeName(
            FormatSymbol(symbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated)),
            symbol.Name,
            symbol.ContainingNamespace.IsGlobalNamespace ? "" : symbol.ContainingNamespace.ToDisplayString());

        public string FormatSymbol(ITypeSymbol symbol)
        {
            // Preserve Roslyn's spelling of compound types; only their namespace qualifiers may need aliases.
            var parts = symbol.ToDisplayParts(DisplayFormat);
            var text = new StringBuilder();
            for (var index = 0; index < parts.Length; index++)
            {
                var part = parts[index];
                var spelling = part.ToString();
                if (part.Symbol is INamespaceSymbol ns && ns.IsGlobalNamespace)
                    for (var next = index + 1; next < parts.Length; next++)
                        if (parts[next].Symbol is INamedTypeSymbol named)
                        {
                            spelling = GetQualifier(named.ContainingAssembly);
                            break;
                        }
                text.Append(spelling);
            }
            return text.ToString();

            string GetQualifier(IAssemblySymbol assembly)
            {
                if (_qualifiers.TryGetValue(assembly, out var qualifier))
                    return qualifier;

                qualifier = "global";
                var reference = _compilation.GetMetadataReference(assembly);
                if (reference != null && !reference.Properties.Aliases.IsDefaultOrEmpty)
                {
                    // Metadata identity does not account for C# namespace/type shadowing. Prefer an explicit alias.
                    foreach (var name in reference.Properties.Aliases)
                        if (name != "global")
                        {
                            qualifier = "@" + name;
                            _externAliases.Add(name);
                            break;
                        }
                }
                _qualifiers.Add(assembly, qualifier);
                return qualifier;
            }
        }
    }
}
