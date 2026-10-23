namespace Lokad.Cli
{
    public interface ITokenizedArguments
    {
    }

    public sealed record LongArg(string Name) : ITokenizedArguments;

    public sealed record LongArgWithValue(string Name, string Value) : ITokenizedArguments;

    public sealed record NumericShortArg(string Value) : ITokenizedArguments;

    public sealed record ShortArgSet(string Options) : ITokenizedArguments;

    public sealed record ShortArgWithValue(char Name, string Value) : ITokenizedArguments;

    public sealed record ValueArg(string Value) : ITokenizedArguments;

    /// <summary>A literal suffix value, excluded from option and command-discriminator inspection.</summary>
    public sealed record LiteralArg(string Value) : ITokenizedArguments;
}
