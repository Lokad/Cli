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

        /// <summary>
        ///     Parses tokenized command-line arguments into an instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="arguments"> Tokenized command-line argument stream. </param>
        /// <returns> Parsed argument object. </returns>
        static abstract T ParseTokenized(TokenizedArguments arguments);

        /// <summary>
        ///     Generates line-by-line command documentation for <typeparamref name="T"/>.
        /// </summary>
        /// <param name="commandName"> Root command name shown in the generated documentation. </param>
        /// <returns> Generated command documentation. </returns>
        static abstract string Document(string commandName);
    }
}
