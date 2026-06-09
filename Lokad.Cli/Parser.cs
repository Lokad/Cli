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
    }
}
