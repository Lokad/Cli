using System.Collections.Generic;

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
        static abstract T Parse(IReadOnlyList<string> arguments);

        /// <summary>
        ///     Tokenizes arguments using this type's generated schema without binding values.
        /// </summary>
        /// <param name="arguments"> Raw command-line argument strings. </param>
        /// <returns> A fresh token cursor for inspection or rewriting. </returns>
        static abstract TokenizedArguments Tokenize(IReadOnlyList<string> arguments);

        /// <summary>Inspects declared help options and command prefixes without binding, conversion or consuming the cursor.</summary>
        /// <param name="arguments">Already classified arguments; lexical validation belongs to tokenization.</param>
        /// <returns>Whether the token stream requests help for this schema.</returns>
        static abstract bool IsHelpRequested(TokenizedArguments arguments);

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
