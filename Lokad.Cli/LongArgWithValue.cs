namespace Lokad.Cli
{
    /// <summary>
    ///     A long option with an inline value, such as <c>--output=file</c>.
    /// </summary>
    /// <param name="Name"> Long option name, without the leading <c>--</c>. </param>
    /// <param name="Value"> Inline option value after <c>=</c>. </param>
    public sealed record LongArgWithValue(string Name, string Value) : ITokenizedArguments;
}
