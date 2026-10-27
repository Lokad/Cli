namespace Lokad.Cli
{
    /// <summary>A classified command-line token.</summary>
    /// <remarks>Generated parsers accept the built-in token records and reject unrecognized implementations with <see cref="ParseException"/>.</remarks>
    public interface ITokenizedArguments
    {
    }

    /// <summary>A long option without a value stored on this token.</summary>
    /// <param name="Name">Long option name without leading dashes.</param>
    public sealed record LongArg(string Name) : ITokenizedArguments;

    /// <summary>A long option and its associated value.</summary>
    /// <param name="Name">Long option name without leading dashes.</param>
    /// <param name="Value">The option's value text.</param>
    public sealed record LongArgWithValue(string Name, string Value) : ITokenizedArguments;

    /// <summary>A numeric shorthand argument such as <c>-10</c>.</summary>
    /// <param name="Value">The digit sequence without the leading dash.</param>
    public sealed record NumericShortArg(string Value) : ITokenizedArguments;

    /// <summary>Short option names without an attached value.</summary>
    /// <param name="Options">Short option names in order, without a leading dash.</param>
    public sealed record ShortArgSet(string Options) : ITokenizedArguments;

    /// <summary>A short option and its associated value.</summary>
    /// <param name="Name">Short option name without a leading dash.</param>
    /// <param name="Value">The option's value text.</param>
    public sealed record ShortArgWithValue(char Name, string Value) : ITokenizedArguments;

    /// <summary>A value that can supply an option operand or a positional argument.</summary>
    /// <param name="Value">The value text.</param>
    public sealed record ValueArg(string Value) : ITokenizedArguments;

    /// <summary>A literal suffix value, excluded from option and command-discriminator inspection.</summary>
    /// <param name="Value">The literal text.</param>
    public sealed record LiteralArg(string Value) : ITokenizedArguments;
}
