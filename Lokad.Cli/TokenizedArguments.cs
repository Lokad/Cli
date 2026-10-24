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

            foreach (var value in PositionalValues())
                return string.Equals(value, prefix, StringComparison.Ordinal);

            return false;
        }

        /// <summary>Inspects help options and positional help commands without advancing this cursor.</summary>
        /// <param name="shortOptions">Declared short help names and their value modes.</param>
        /// <param name="longOptions">Declared long help names and their value modes.</param>
        /// <param name="commands">The selected entrypoint's command tree, including positional help markers.</param>
        /// <returns>Whether a declared help occurrence is present; help-looking text inside values or literal suffixes is excluded.</returns>
        public bool IsHelpRequested(IReadOnlyDictionary<char, OptionValueMode> shortOptions,
            IReadOnlyDictionary<string, OptionValueMode> longOptions, CommandBoundary commands)
        {
            if (shortOptions == null) throw new ArgumentNullException(nameof(shortOptions));
            if (longOptions == null) throw new ArgumentNullException(nameof(longOptions));
            if (commands == null) throw new ArgumentNullException(nameof(commands));

            foreach (var argument in _arguments)
            {
                if (argument is LongArg longArg && longOptions.ContainsKey(longArg.Name))
                    return true;
                if (argument is LongArgWithValue longValue && longOptions.TryGetValue(longValue.Name, out var longMode) && longMode != OptionValueMode.None)
                    return true;
                if (argument is ShortArgWithValue shortValue && shortOptions.TryGetValue(shortValue.Name, out var shortMode) && shortMode != OptionValueMode.None)
                    return true;
                if (argument is ShortArgSet group)
                    foreach (var name in group.Options)
                        if (shortOptions.ContainsKey(name))
                            return true;
            }
            if (!commands.HasHelpPrefixes)
                return false;
            return HasHelpPrefix();

            bool HasHelpPrefix()
            {
                var longNames = new HashSet<string>(StringComparer.Ordinal);
                var shortNames = new HashSet<char>();
                foreach (var argument in _arguments)
                {
                    switch (argument)
                    {
                        case LongArg option: longNames.Add(option.Name); break;
                        case LongArgWithValue option: longNames.Add(option.Name); break;
                        case ShortArgWithValue option: shortNames.Add(option.Name); break;
                        case ShortArgSet group:
                            foreach (var name in group.Options)
                                shortNames.Add(name);
                            break;
                    }
                }
                var values = new List<string>(PositionalValues());
                var visited = new HashSet<(CommandBoundary Command, int Position)>();
                // The caller has selected this entrypoint. Consume its positional prefix before inspecting children.
                var start = 0;
                if (commands.Prefix != null && !TryMatch(commands, 0, out start))
                    return false;
                return Visit(commands, start);

                bool Visit(CommandBoundary command, int position)
                {
                    if (!command.HasHelpPrefixes || !visited.Add((command, position)))
                        return false;
                    if (command.IsHelpPrefix)
                        return true;
                    var hasExplicitChoice = false;
                    foreach (var child in command.Choices)
                    {
                        if (child.IsDefault || !TryMatch(child, position, out var next))
                            continue;
                        hasExplicitChoice = true;
                        if (Visit(child, next))
                            return true;
                    }
                    if (hasExplicitChoice)
                        return false;
                    foreach (var child in command.Choices)
                        if (child.IsDefault && Visit(child, position))
                            return true;
                    return false;
                }

                bool TryMatch(CommandBoundary command, int position, out int next)
                {
                    next = position;
                    if (command.Prefix != null)
                    {
                        if (position >= values.Count || !string.Equals(command.Prefix, values[position], StringComparison.Ordinal))
                            return false;
                        next++;
                        return true;
                    }
                    return command.IsDefault ||
                        (command.LongOption != null && longNames.Contains(command.LongOption)) ||
                        (command.ShortOption != '\0' && shortNames.Contains(command.ShortOption));
                }
            }
        }

        private IEnumerable<string> PositionalValues()
        {
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

                yield return valueArg.Value;
            }
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
