namespace Lokad.Cli
{
    /// <summary>
    ///     A long option without an inline value, such as <c>--output</c>.
    /// </summary>
    /// <param name="Name"> Long option name, without the leading <c>--</c>. </param>
    public sealed record LongArg(string Name) : ITokenizedArguments;
}
