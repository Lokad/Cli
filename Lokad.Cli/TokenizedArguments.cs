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

        // Binding can inspect a token with Peek or consume it with Shift before checking its prefix position.
        private int _currentTokenIndex = -1;

        /// <summary>
        ///     Positions of selected prefixes that should be ignored by the binding phase.
        /// </summary>
        private readonly Queue<int> _skipPositional = new Queue<int>();

        private HashSet<string>? _selectedLongOptions;
        private HashSet<char>? _selectedShortOptions;

        /// <summary>
        ///     Initializes a token stream from a pre-tokenized argument list.
        /// </summary>
        /// <param name="arguments"> Tokenized arguments in original command-line order. </param>
        public TokenizedArguments(IReadOnlyList<ITokenizedArguments> arguments)
        {
            _arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
        }

        /// <summary>Registers the option selectors of a selected command without consuming the cursor.</summary>
        /// <param name="longOption">The command's long selector, or null.</param>
        /// <param name="shortOption">The command's short selector, or '\0'.</param>
        public void SelectDiscriminator(string? longOption, char shortOption)
        {
            if (longOption != null)
                (_selectedLongOptions ??= new HashSet<string>(StringComparer.Ordinal)).Add(longOption);
            if (shortOption != '\0')
                (_selectedShortOptions ??= new HashSet<char>()).Add(shortOption);
        }

        /// <summary>Whether a bare long option belongs to the selected command path; binding actual members takes precedence.</summary>
        /// <param name="option">Long option name without dashes.</param>
        /// <returns>Whether binding may ignore the bare selector.</returns>
        public bool IsSelectedLongOption(string option) => _selectedLongOptions?.Contains(option) ?? false;

        /// <summary>Whether a bare short option belongs to the selected command path; binding actual members takes precedence.</summary>
        /// <param name="option">Short option name without a dash.</param>
        /// <returns>Whether binding may ignore the bare selector.</returns>
        public bool IsSelectedShortOption(char option) => _selectedShortOptions?.Contains(option) ?? false;

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

            foreach (var positional in PositionalTokens())
                return string.Equals(positional.Token.Value, prefix, StringComparison.Ordinal);

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
                var values = new List<string>();
                foreach (var positional in PositionalTokens())
                    values.Add(positional.Token.Value);
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

        private IEnumerable<(int Index, ValueArg Token)> PositionalTokens()
        {
            var skipPositional = _skipPositional.ToArray();
            var skipIndex = 0;
            for (var index = 0; index < _arguments.Count; index++)
            {
                if (!(_arguments[index] is ValueArg valueArg))
                    continue;

                if (skipIndex < skipPositional.Length &&
                    index == skipPositional[skipIndex])
                {
                    skipIndex++;
                    continue;
                }

                yield return (index, valueArg);
            }
        }

        /// <summary>
        ///     Selects the next matching positional token to skip when binding, after any previously selected prefixes.
        /// </summary>
        /// <param name="value"> Positional value to skip. </param>
        public void SkipPositional(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            var after = -1;
            foreach (var index in _skipPositional)
                after = index;
            foreach (var positional in PositionalTokens())
            {
                if (positional.Index <= after || !string.Equals(positional.Token.Value, value, StringComparison.Ordinal))
                    continue;
                _skipPositional.Enqueue(positional.Index);
                return;
            }
        }

        /// <summary>
        ///     Returns whether the token most recently inspected with Peek or Shift is a selected prefix to skip.
        /// </summary>
        /// <param name="value"> Positional value about to be bound. </param>
        /// <returns> <c>true</c> when the value was scheduled to be skipped; otherwise <c>false</c>. </returns>
        public bool MaybeSkipPositional(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            while (_skipPositional.Count > 0 && _skipPositional.Peek() < _currentTokenIndex)
                _skipPositional.Dequeue();
            if (_skipPositional.Count == 0 ||
                _skipPositional.Peek() != _currentTokenIndex ||
                !(_arguments[_currentTokenIndex] is ValueArg positional) ||
                !string.Equals(positional.Value, value, StringComparison.Ordinal))
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
            _currentTokenIndex = _position;
            return _position < _arguments.Count ? _arguments[_position] : null;
        }

        /// <summary>
        ///     Returns the next unconsumed token and advances the cursor.
        /// </summary>
        /// <returns> Consumed token, or <c>null</c> when the stream is exhausted. </returns>
        public ITokenizedArguments? Shift()
        {
            _currentTokenIndex = _position;
            if (_position >= _arguments.Count)
                return null;

            return _arguments[_position++];
        }
    }
}
