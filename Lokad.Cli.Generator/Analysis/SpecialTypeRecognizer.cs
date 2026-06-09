using Microsoft.CodeAnalysis;

namespace Lokad.Cli.Generator.Analysis
{
    internal static class SpecialTypeRecognizer
    {
        public static TypeRecognizer Make(SpecialType specialType) =>
            (_, type) => type.SpecialType == specialType;
    }
}
