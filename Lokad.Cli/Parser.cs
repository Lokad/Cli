namespace Lokad.Cli
{
    /// <summary>
    ///     Runtime facade for generated command-line parsers.
    /// </summary>
    public static class Parser
    {
        /// <summary>
        ///     Parses command-line arguments into the requested generated argument type.
        /// </summary>
        /// <param name="arguments"> Raw command-line argument strings. </param>
        /// <typeparam name="T"> Generated argument type. </typeparam>
        /// <returns> Parsed argument object. </returns>
        public static T Parse<T>(string[] arguments)
            where T : ICliParsed<T>
        {
            return T.Parse(arguments);
        }

        /// <summary>
        ///     Parses tokenized command-line arguments into the requested generated argument type.
        /// </summary>
        /// <param name="arguments"> Tokenized command-line argument stream. </param>
        /// <typeparam name="T"> Generated argument type. </typeparam>
        /// <returns> Parsed argument object. </returns>
        public static T ParseTokenized<T>(TokenizedArguments arguments)
            where T : ICliParsed<T>
        {
            return T.ParseTokenized(arguments);
        }

        /// <summary>
        ///     Generates line-by-line documentation for the requested generated argument type.
        /// </summary>
        /// <param name="commandName"> Root command name shown in the generated documentation. </param>
        /// <typeparam name="T"> Generated argument type. </typeparam>
        /// <returns> Generated command documentation. </returns>
        public static string Document<T>(string commandName)
            where T : ICliParsed<T>
        {
            return T.Document(commandName);
        }
    }
}
