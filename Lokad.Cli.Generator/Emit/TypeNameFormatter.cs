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
        private readonly Dictionary<INamedTypeSymbol, string> _qualifiers =
            new Dictionary<INamedTypeSymbol, string>(SymbolEqualityComparer.Default);
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
                            spelling = GetQualifier(named.OriginalDefinition);
                            break;
                        }
                text.Append(spelling);
            }
            return text.ToString();

            string GetQualifier(INamedTypeSymbol type)
            {
                if (_qualifiers.TryGetValue(type, out var qualifier))
                    return qualifier;

                qualifier = "global";
                var reference = _compilation.GetMetadataReference(type.ContainingAssembly);
                if (reference != null && !reference.Properties.Aliases.IsDefaultOrEmpty)
                {
                    var globallyVisible = false;
                    string? alias = null;
                    foreach (var name in reference.Properties.Aliases)
                        if (name == "global")
                            globallyVisible = true;
                        else
                            alias ??= name;

                    if (alias != null && (!globallyVisible || !SymbolEqualityComparer.Default.Equals(
                        _compilation.GetTypeByMetadataName(GetMetadataName()), type)))
                    {
                        qualifier = "@" + alias;
                        _externAliases.Add(alias);
                    }
                }
                _qualifiers.Add(type, qualifier);
                return qualifier;

                string GetMetadataName()
                {
                    var name = type.MetadataName;
                    for (var parent = type.ContainingType; parent != null; parent = parent.ContainingType)
                        name = parent.MetadataName + "+" + name;
                    for (var parent = type.ContainingNamespace; !parent.IsGlobalNamespace; parent = parent.ContainingNamespace)
                        name = parent.MetadataName + "." + name;
                    return name;
                }
            }
        }
    }
}
