namespace Lokad.Cli
{
    /// <summary>
    ///     A short option with an inline value, such as <c>-o=file</c>.
    /// </summary>
    /// <param name="Name"> Short option name, without the leading <c>-</c>. </param>
    /// <param name="Value"> Inline option value after <c>=</c>. </param>
    public sealed record ShortArgWithValue(char Name, string Value) : ITokenizedArguments;
}
