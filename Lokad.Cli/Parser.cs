using System;
using System.Collections.Generic;

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
        ///     Parses command-line arguments into the requested generated argument type.
        /// </summary>
        /// <param name="arguments"> Raw command-line argument strings. </param>
        /// <typeparam name="T"> Generated argument type. </typeparam>
        /// <returns> Parsed argument object. </returns>
        public static T Parse<T>(IEnumerable<string> arguments)
            where T : ICliParsed<T>
        {
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));

            if (arguments is IReadOnlyList<string> list)
                return Parse<T>(list);

            return Parse<T>(new List<string>(arguments));
        }

        /// <summary>
        ///     Parses command-line arguments into the requested generated argument type.
        /// </summary>
        /// <param name="arguments"> Raw command-line argument strings. </param>
        /// <typeparam name="T"> Generated argument type. </typeparam>
        /// <returns> Parsed argument object. </returns>
        public static T Parse<T>(IReadOnlyList<string> arguments)
            where T : ICliParsed<T>
        {
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));

            return T.Parse(arguments);
        }

        /// <summary>
        ///     Parses command-line arguments into the requested generated argument type.
        /// </summary>
        /// <param name="arguments"> Raw command-line argument strings. </param>
        /// <typeparam name="T"> Generated argument type. </typeparam>
        /// <returns> Parsed argument object. </returns>
        public static T Parse<T>(ReadOnlySpan<string> arguments)
            where T : ICliParsed<T>
        {
            return Parse<T>(arguments.ToArray());
        }

        /// <summary>
        ///     Tokenizes arguments using the requested type's generated schema without binding values.
        /// </summary>
        /// <param name="arguments"> Raw command-line argument strings. </param>
        /// <typeparam name="T"> Generated argument type. </typeparam>
        /// <returns> A fresh token cursor for inspection or rewriting. </returns>
        public static TokenizedArguments Tokenize<T>(IReadOnlyList<string> arguments)
            where T : ICliParsed<T>
        {
            return T.Tokenize(arguments);
        }

        /// <summary>Detects declared help before binding or conversion. Tokenizes the entire input, so lexical errors take precedence.</summary>
        /// <param name="arguments">Raw command-line arguments.</param>
        /// <typeparam name="T">Generated argument type.</typeparam>
        /// <returns>Whether a declared help option or help command prefix occurs.</returns>
        public static bool IsHelpRequested<T>(IReadOnlyList<string> arguments)
            where T : ICliParsed<T>
        {
            return T.IsHelpRequested(T.Tokenize(arguments));
        }

        /// <summary>Detects declared help in classified tokens without binding, conversion or consuming the cursor.</summary>
        /// <param name="arguments">Already classified arguments; lexical validation belongs to tokenization.</param>
        /// <typeparam name="T">Generated argument type.</typeparam>
        /// <returns>Whether a declared help option or help command prefix occurs.</returns>
        public static bool IsHelpRequested<T>(TokenizedArguments arguments)
            where T : ICliParsed<T>
        {
            return T.IsHelpRequested(arguments);
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
