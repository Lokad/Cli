using Microsoft.CodeAnalysis;

namespace Lokad.Cli.Generator.Emit
{
    public sealed class TypeName
    {
        private static readonly SymbolDisplayFormat DisplayFormat =
            SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
                SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions |
                SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

        public TypeName(string fullName, string name, string ns)
        {
            FullName = fullName;
            Name = name;
            Namespace = ns;
        }

        public string FullName { get; }

        public string Name { get; }

        public string Namespace { get; }

        public static TypeName FromSymbol(ITypeSymbol symbol) => new TypeName(
            FormatSymbol(symbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated)),
            symbol.Name,
            symbol.ContainingNamespace.IsGlobalNamespace ? "" : symbol.ContainingNamespace.ToDisplayString());

        public static string FormatSymbol(ITypeSymbol symbol) => symbol.ToDisplayString(DisplayFormat);
    }
}
