using System;

namespace Lokad.Cli.Generator.Emit
{
    [Flags]
    public enum CliParserMethods
    {
        None = 0,
        Parse = 1,
        Tokenize = 2,
        ParseTokenized = 4,
        Document = 8
    }
}
