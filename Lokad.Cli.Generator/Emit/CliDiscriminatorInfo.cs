namespace Lokad.Cli.Generator.Emit
{
    public sealed class CliDiscriminatorInfo
    {
        public CliDiscriminatorInfo(string display, string? prefix, string? longOption, char shortOption)
        {
            Display = display;
            Prefix = prefix;
            LongOption = longOption;
            ShortOption = shortOption;
        }

        public string Display { get; }

        public string? Prefix { get; }

        public string? LongOption { get; }

        public char ShortOption { get; }
    }
}
