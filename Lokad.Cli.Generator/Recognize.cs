using Lokad.Cli.Generator.Analysis;
using Microsoft.CodeAnalysis;

namespace Lokad.Cli.Generator
{
    public static class Recognize
    {
        public static TypeRecognizer Type(SpecialType special) =>
            SpecialTypeRecognizer.Make(special);

        public static TypeRecognizer String = Type(SpecialType.System_String);

        public static TypeRecognizer Type(string name) =>
            name == "string" ? String : TypeSymbolRecognizer.IsNamed(name);
    }
}
