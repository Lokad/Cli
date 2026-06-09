namespace Lokad.Cli
{
    /// <summary>
    ///     A positional value argument.
    /// </summary>
    /// <param name="Value"> Raw positional value. </param>
    public sealed record ValueArg(string Value) : ITokenizedArguments;
}
