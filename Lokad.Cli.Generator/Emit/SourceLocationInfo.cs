namespace Lokad.Cli.Generator.Emit
{
    public sealed class SourceLocationInfo
    {
        public SourceLocationInfo(string path, int line, int column)
        {
            Path = path;
            Line = line;
            Column = column;
        }

        public string Path { get; }

        public int Line { get; }

        public int Column { get; }
    }
}
