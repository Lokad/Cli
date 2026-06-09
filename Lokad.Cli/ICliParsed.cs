namespace Lokad.Cli
{
    /// <summary>
    ///     Implemented by generated argument types that can parse themselves from command-line arguments.
    /// </summary>
    /// <typeparam name="T"> Concrete parsed argument type. </typeparam>
    public interface ICliParsed<T>
    {
        /// <summary>
        ///     Parses command-line arguments into an instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="arguments"> Raw command-line argument strings. </param>
        /// <returns> Parsed argument object. </returns>
        static abstract T Parse(string[] arguments);
    }
}
