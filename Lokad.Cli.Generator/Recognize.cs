using Lokad.Cli.Generator.Analysis;

namespace Lokad.Cli.Generator
{
    public static class Recognize
    {
        public static TypeRecognizer Type(string name) =>
            TypeSymbolRecognizer.IsNamed(name);
    }
}
