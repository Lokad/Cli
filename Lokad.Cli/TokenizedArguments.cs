using System;
using System.Collections.Generic;

namespace Lokad.Cli
{
    /// <summary>
    ///     Mutable cursor over tokenized command-line arguments.
    /// </summary>
    public sealed class TokenizedArguments
    {
        /// <summary>
        ///     Complete token list produced by tokenization.
        /// </summary>
        private readonly IReadOnlyList<ITokenizedArguments> _arguments;

        /// <summary>
        ///     Current cursor position used by <see cref="Peek"/> and <see cref="Shift"/>.
        /// </summary>
        private int _position;

        /// <summary>
        ///     Positional values that should be ignored by the binding phase.
        /// </summary>
        private readonly Queue<string> _skipPositional = new Queue<string>();

        /// <summary>
        ///     Initializes a token stream from a pre-tokenized argument list.
        /// </summary>
        /// <param name="arguments"> Tokenized arguments in original command-line order. </param>
        public TokenizedArguments(IReadOnlyList<ITokenizedArguments> arguments)
        {
            _arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
        }

        /// <summary>
        ///     Returns whether the token stream contains a specific short option.
        /// </summary>
        /// <param name="option"> Short option name, without the leading <c>-</c>. </param>
        /// <returns> <c>true</c> when the option is present; otherwise <c>false</c>. </returns>
        public bool HasShortOption(char option)
        {
            foreach (var argument in _arguments)
            {
                if (argument is ShortArgSet shortArgSet &&
                    shortArgSet.Options.IndexOf(option) >= 0)
                    return true;

                if (argument is ShortArgWithValue shortArgWithValue &&
                    shortArgWithValue.Name == option)
                    return true;
            }

            return false;
        }

        /// <summary>
        ///     Returns whether the token stream contains a specific long option.
        /// </summary>
        /// <param name="option"> Long option name, without the leading <c>--</c>. </param>
        /// <returns> <c>true</c> when the option is present; otherwise <c>false</c>. </returns>
        public bool HasLongOption(string option)
        {
            if (option == null) throw new ArgumentNullException(nameof(option));

            foreach (var argument in _arguments)
            {
                if (argument is LongArg longArg &&
                    string.Equals(longArg.Name, option, StringComparison.Ordinal))
                    return true;

                if (argument is LongArgWithValue longArgWithValue &&
                    string.Equals(longArgWithValue.Name, option, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        /// <summary>
        ///     Returns whether the first positional token has the specified value.
        /// </summary>
        /// <param name="prefix"> Expected positional discriminator value. </param>
        /// <returns> <c>true</c> when the first positional token matches; otherwise <c>false</c>. </returns>
        public bool HasPositionalPrefix(string prefix)
        {
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));

            var skipPositional = _skipPositional.ToArray();
            var skipIndex = 0;
            foreach (var argument in _arguments)
            {
                if (!(argument is ValueArg valueArg))
                    continue;

                if (skipIndex < skipPositional.Length &&
                    string.Equals(valueArg.Value, skipPositional[skipIndex], StringComparison.Ordinal))
                {
                    skipIndex++;
                    continue;
                }

                return string.Equals(valueArg.Value, prefix, StringComparison.Ordinal);
            }

            return false;
        }

        /// <summary>
        ///     Adds a positional value to skip when binding positional arguments.
        /// </summary>
        /// <param name="value"> Positional value to skip. </param>
        public void SkipPositional(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            _skipPositional.Enqueue(value);
        }

        /// <summary>
        ///     Returns whether the supplied positional value should be skipped by binding.
        /// </summary>
        /// <param name="value"> Positional value about to be bound. </param>
        /// <returns> <c>true</c> when the value was scheduled to be skipped; otherwise <c>false</c>. </returns>
        public bool MaybeSkipPositional(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (_skipPositional.Count == 0 ||
                !string.Equals(_skipPositional.Peek(), value, StringComparison.Ordinal))
                return false;

            _skipPositional.Dequeue();
            return true;
        }

        /// <summary>
        ///     Returns the next unconsumed token without advancing the cursor.
        /// </summary>
        /// <returns> Next token, or <c>null</c> when the stream is exhausted. </returns>
        public ITokenizedArguments? Peek()
        {
            return _position < _arguments.Count ? _arguments[_position] : null;
        }

        /// <summary>
        ///     Returns the next unconsumed token and advances the cursor.
        /// </summary>
        /// <returns> Consumed token, or <c>null</c> when the stream is exhausted. </returns>
        public ITokenizedArguments? Shift()
        {
            if (_position >= _arguments.Count)
                return null;

            return _arguments[_position++];
        }
    }
}
