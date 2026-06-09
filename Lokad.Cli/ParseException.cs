using System;

namespace Lokad.Cli
{
    /// <summary>
    ///     Exception thrown when command-line arguments cannot be parsed.
    /// </summary>
    public sealed class ParseException : Exception
    {
        /// <summary>
        ///     Initializes a parsing exception without a message.
        /// </summary>
        public ParseException()
        {
        }

        /// <summary>
        ///     Initializes a parsing exception with a user-facing error message.
        /// </summary>
        /// <param name="message"> Description of the parsing error. </param>
        public ParseException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a parsing exception with a user-facing error message and inner exception.
        /// </summary>
        /// <param name="message"> Description of the parsing error. </param>
        /// <param name="innerException"> Exception that caused this parsing error. </param>
        public ParseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
