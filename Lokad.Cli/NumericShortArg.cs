namespace Lokad.Cli
{
    /// <summary>
    ///     A short numeric argument such as <c>-10</c>.
    /// </summary>
    /// <param name="Value"> Numeric value text, without the leading <c>-</c>. </param>
    public sealed record NumericShortArg(string Value) : ITokenizedArguments;
}
