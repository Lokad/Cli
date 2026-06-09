namespace Lokad.Cli
{
    /// <summary>
    ///     A compact set of short options such as <c>-abc</c>.
    /// </summary>
    /// <param name="Options"> Short option names, without the leading <c>-</c>. </param>
    public sealed record ShortArgSet(string Options) : ITokenizedArguments;
}
