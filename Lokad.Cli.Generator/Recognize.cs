using Lokad.Cli.Generator.Analysis;

namespace Lokad.Cli.Generator
{
    public static class Recognize
    {
        public static TypeRecognizer Type(string name) =>
            TypeSymbolRecognizer.FromResolver(compilation => compilation.GetTypeByMetadataName(name));

        public static TypeRecognizer CliType(string name) =>
            TypeSymbolRecognizer.FromResolver(compilation => compilation.GetTypeByMetadataName("Lokad.Cli.ICliParsed`1")?
                .ContainingAssembly.GetTypeByMetadataName(name));
    }
}
