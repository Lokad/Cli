using Microsoft.CodeAnalysis;

namespace Lokad.Cli.Generator
{
    public delegate bool TypeRecognizer(Compilation compilation, ITypeSymbol type);
}
