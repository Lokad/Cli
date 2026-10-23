using System;
using System.Collections.Generic;

namespace Lokad.Cli
{
    /// <summary>
    ///     Converts raw command-line argument strings into classified tokens.
    /// </summary>
    public static class ArgumentTokenizer
    {
        /// <summary>
        ///     Tokenizes raw command-line arguments using the option names known to a generated parser.
        /// </summary>
        /// <param name="arguments"> Raw command-line argument strings. </param>
        /// <param name="shortOptions"> Recognized short option names, without leading <c>-</c>. </param>
        /// <param name="longOptions"> Recognized long option names, without leading <c>--</c>. </param>
        /// <param name="hasShortNumericArguments">
        ///     Indicates whether numeric short-argument forms such as <c>-10</c> should be recognized.
        /// </param>
        /// <returns> Tokenized argument stream. </returns>
        /// <remarks>
        ///     When a multi-character short argument such as <c>-abc</c> is encountered and the first
        ///     character (<c>a</c>) is a recognized short option but one or more of the remaining
        ///     characters are not, the argument is interpreted as option <c>-a</c> with an inline
        ///     value of <c>bc</c>, equivalent to <c>-a=bc</c>.  This mirrors the convention used
        ///     by tools such as <c>tar</c> and <c>grep</c>.
        /// </remarks>
        public static TokenizedArguments Tokenize(
            IReadOnlyList<string> arguments,
            char[] shortOptions,
            string[] longOptions,
            bool hasShortNumericArguments)
        {
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));
            if (shortOptions == null) throw new ArgumentNullException(nameof(shortOptions));
            if (longOptions == null) throw new ArgumentNullException(nameof(longOptions));

            var shortModes = new Dictionary<char, OptionValueMode>();
            foreach (var name in shortOptions)
                shortModes[name] = OptionValueMode.None;
            var longModes = new Dictionary<string, OptionValueMode>(StringComparer.Ordinal);
            foreach (var name in longOptions)
                longModes[name] = OptionValueMode.None;

            return Tokenize(arguments, shortModes, longModes, hasShortNumericArguments,
                ShortOptionSyntax.GroupedValues, allowSingleDashLongOptions: true);
        }

        /// <summary>Tokenizes arguments with generated option arities and explicit lexical policies.</summary>
        /// <param name="arguments">Raw command-line arguments.</param>
        /// <param name="shortOptions">Short names and their value modes.</param>
        /// <param name="longOptions">Long names and their value modes.</param>
        /// <param name="hasShortNumericArguments">Whether numeric short forms such as <c>-10</c> are recognized.</param>
        /// <param name="shortOptionSyntax">How short groups supply values.</param>
        /// <param name="allowSingleDashLongOptions">Whether known long names also accept one leading dash.</param>
        /// <returns>A fresh token cursor.</returns>
        public static TokenizedArguments Tokenize(
            IReadOnlyList<string> arguments,
            IReadOnlyDictionary<char, OptionValueMode> shortOptions,
            IReadOnlyDictionary<string, OptionValueMode> longOptions,
            bool hasShortNumericArguments,
            ShortOptionSyntax shortOptionSyntax,
            bool allowSingleDashLongOptions)
            => Tokenize(arguments, shortOptions, longOptions, hasShortNumericArguments,
                shortOptionSyntax, allowSingleDashLongOptions, false, CommandBoundary.Empty);

        /// <summary>Tokenizes arguments with command selectors and literal-suffix boundaries.</summary>
        /// <param name="arguments">Raw command-line arguments.</param>
        /// <param name="shortOptions">Short names and their value modes.</param>
        /// <param name="longOptions">Long names and their value modes.</param>
        /// <param name="hasShortNumericArguments">Whether numeric short forms are recognized.</param>
        /// <param name="shortOptionSyntax">How short groups supply values.</param>
        /// <param name="allowSingleDashLongOptions">Whether known long names accept one leading dash.</param>
        /// <param name="stopAtFirstOperand">Whether the first operand starts a literal suffix, excluding command prefixes and option values.</param>
        /// <param name="boundary">Generated command selectors and explicit boundaries.</param>
        /// <returns>A fresh token cursor; literal suffix strings are represented by <see cref="LiteralArg"/>.</returns>
        public static TokenizedArguments Tokenize(
            IReadOnlyList<string> arguments,
            IReadOnlyDictionary<char, OptionValueMode> shortOptions,
            IReadOnlyDictionary<string, OptionValueMode> longOptions,
            bool hasShortNumericArguments,
            ShortOptionSyntax shortOptionSyntax,
            bool allowSingleDashLongOptions,
            bool stopAtFirstOperand,
            CommandBoundary boundary)
        {
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));
            if (shortOptions == null) throw new ArgumentNullException(nameof(shortOptions));
            if (longOptions == null) throw new ArgumentNullException(nameof(longOptions));
            if (boundary == null) throw new ArgumentNullException(nameof(boundary));
            if (shortOptionSyntax != ShortOptionSyntax.GroupedValues && shortOptionSyntax != ShortOptionSyntax.AttachedValues)
                throw new ArgumentOutOfRangeException(nameof(shortOptionSyntax));
            if (shortOptionSyntax != ShortOptionSyntax.AttachedValues)
            {
                if (boundary.HasShortBoundaries)
                    throw new ArgumentException("Short option boundaries require AttachedValues syntax.", nameof(boundary));
                foreach (var mode in shortOptions.Values)
                    if (mode == OptionValueMode.OptionalAttached)
                        throw new ArgumentException("Optional short values require AttachedValues syntax.", nameof(shortOptions));
            }

            var tokens = new List<ITokenizedArguments>(arguments.Count);
            var pendingValues = new Queue<int>();
            var classified = new List<ITokenizedArguments>(2);
            var valuesOnly = false;
            var state = stopAtFirstOperand || boundary.HasBoundaries
                ? new ArgumentBoundaryState(boundary, stopAtFirstOperand) : null;

            foreach (var argument in arguments)
            {
                if (state?.IsLiteral == true)
                {
                    tokens.Add(new LiteralArg(argument));
                    continue;
                }
                if (pendingValues.Count > 0 && GetValueMode(tokens[pendingValues.Peek()]) == OptionValueMode.Verbatim)
                {
                    SupplyValue(argument);
                    continue;
                }

                if (!valuesOnly && argument == "--")
                {
                    valuesOnly = true;
                    continue;
                }

                classified.Clear();
                if (valuesOnly ||
                    (!TryTokenizeLong(argument, longOptions, singleDash: false, classified) &&
                     !(allowSingleDashLongOptions && TryTokenizeLong(argument, longOptions, singleDash: true, classified)) &&
                     !TryTokenizeShort(argument, shortOptions, hasShortNumericArguments, shortOptionSyntax, classified)))
                    classified.Add(new ValueArg(argument));

                foreach (var token in classified)
                {
                    if (pendingValues.Count > 0)
                    {
                        if (token is ValueArg value)
                        {
                            SupplyValue(value.Value);
                            continue;
                        }
                        // Binding will report the missing operand. Do not apply its remaining raw-value modes to unrelated options.
                        pendingValues.Clear();
                    }

                    if (token is ValueArg operand)
                    {
                        var literal = state != null && !state.TryConsumePrefix(operand.Value) && state.PositionalValue(valuesOnly);
                        tokens.Add(literal ? new LiteralArg(operand.Value) : token);
                        continue;
                    }

                    state?.ObserveOption(token);
                    if (token is ShortArgSet group)
                    {
                        var start = 0;
                        for (var i = 0; i < group.Options.Length; i++)
                        {
                            var name = group.Options[i];
                            if (!NeedsFollowingValue(shortOptions[name]))
                                continue;
                            if (i > start)
                                tokens.Add(new ShortArgSet(group.Options.Substring(start, i - start)));
                            pendingValues.Enqueue(tokens.Count);
                            tokens.Add(new ShortArgSet(name.ToString()));
                            start = i + 1;
                        }
                        if (start == 0)
                            tokens.Add(group);
                        else if (start < group.Options.Length)
                            tokens.Add(new ShortArgSet(group.Options.Substring(start)));
                    }
                    else
                    {
                        if (token is LongArg && NeedsFollowingValue(GetValueMode(token)))
                            pendingValues.Enqueue(tokens.Count);
                        tokens.Add(token);
                        if (token is LongArgWithValue || token is ShortArgWithValue)
                            state?.OptionValueConsumed(token);
                    }
                }
            }

            return new TokenizedArguments(tokens);

            static bool NeedsFollowingValue(OptionValueMode mode) =>
                mode == OptionValueMode.Required || mode == OptionValueMode.Verbatim;

            OptionValueMode GetValueMode(ITokenizedArguments token) => token switch
            {
                LongArg option => longOptions[option.Name],
                ShortArgSet option => shortOptions[option.Options[0]],
                _ => OptionValueMode.None
            };

            void SupplyValue(string value)
            {
                var index = pendingValues.Dequeue();
                tokens[index] = tokens[index] switch
                {
                    LongArg option => new LongArgWithValue(option.Name, value),
                    ShortArgSet option => new ShortArgWithValue(option.Options[0], value),
                    _ => throw new InvalidOperationException("Expected an option awaiting a value.")
                };
                state?.OptionValueConsumed(tokens[index]);
            }
        }

        /// <summary>
        ///     Attempts to classify an argument as a known long option.
        /// </summary>
        /// <param name="argument"> Raw argument string. </param>
        /// <param name="longOptions"> Recognized long option names. </param>
        /// <param name="singleDash">Whether to recognize the optional single-dash spelling.</param>
        /// <param name="tokens"> Destination token list. </param>
        /// <returns>
        ///     <c>true</c> when <paramref name="argument"/> was a long option; otherwise <c>false</c>.
        /// </returns>
        private static bool TryTokenizeLong(
            string argument,
            IReadOnlyDictionary<string, OptionValueMode> longOptions,
            bool singleDash,
            List<ITokenizedArguments> tokens)
        {
            if (argument.Length <= 2 || (singleDash
                ? !argument.StartsWith("-", StringComparison.Ordinal) || argument.StartsWith("--", StringComparison.Ordinal)
                : !argument.StartsWith("--", StringComparison.Ordinal)))
                return false;

            var prefixLength = singleDash ? 1 : 2;
            var separator = argument.IndexOf('=');
            var name = separator < 0 ? argument.Substring(prefixLength) : argument.Substring(prefixLength, separator - prefixLength);

            if (!longOptions.ContainsKey(name))
            {
                if (singleDash)
                    return false;
                throw new ParseException($"unknown option `--{name}`");
            }

            if (separator < 0)
            {
                tokens.Add(new LongArg(name));
            }
            else
            {
                tokens.Add(new LongArgWithValue(name, argument.Substring(separator + 1)));
            }

            return true;
        }

        private static bool TryTokenizeShort(
            string argument,
            IReadOnlyDictionary<char, OptionValueMode> shortOptions,
            bool hasShortNumericArguments,
            ShortOptionSyntax syntax,
            List<ITokenizedArguments> tokens)
        {
            if (!argument.StartsWith("-", StringComparison.Ordinal) ||
                argument.StartsWith("--", StringComparison.Ordinal) ||
                argument.Length <= 1)
                return false;

            if (hasShortNumericArguments && IsDigits(argument, 1))
            {
                tokens.Add(new NumericShortArg(argument.Substring(1)));
                return true;
            }

            var set = argument.Substring(1);
            if (syntax == ShortOptionSyntax.AttachedValues)
            {
                for (var i = 0; i < set.Length; i++)
                {
                    var option = set[i];
                    if (!shortOptions.TryGetValue(option, out var mode))
                        throw new ParseException($"unknown short option `-{option}`");
                    if (mode == OptionValueMode.None)
                        continue;

                    if (i > 0)
                        tokens.Add(new ShortArgSet(set.Substring(0, i)));
                    var valueStart = i + 1;
                    if (valueStart < set.Length)
                    {
                        if (set[valueStart] == '=')
                            valueStart++;
                        tokens.Add(new ShortArgWithValue(option, set.Substring(valueStart)));
                    }
                    else
                        tokens.Add(new ShortArgSet(option.ToString()));
                    return true;
                }

                tokens.Add(new ShortArgSet(set));
                return true;
            }

            var separator = set.IndexOf('=');
            if (separator >= 0)
            {
                if (separator == 1)
                {
                    var option = set[0];
                    if (!shortOptions.ContainsKey(option))
                        throw new ParseException($"unknown short option `-{option}`");

                    tokens.Add(new ShortArgWithValue(option, set.Substring(separator + 1)));
                    return true;
                }

                throw new ParseException($"unexpected = in short option list `{argument}`");
            }

            if (AllShortOptionsKnown(set, shortOptions))
            {
                tokens.Add(new ShortArgSet(set));
                return true;
            }

            if (set.Length > 1 && shortOptions.ContainsKey(set[0]))
            {
                tokens.Add(new ShortArgWithValue(set[0], set.Substring(1)));
                return true;
            }

            ThrowUnknownShortOption(set, shortOptions);
            return true;
        }

        private static bool AllShortOptionsKnown(string options, IReadOnlyDictionary<char, OptionValueMode> shortOptions)
        {
            foreach (var option in options)
                if (!shortOptions.ContainsKey(option))
                    return false;

            return true;
        }

        private static void ThrowUnknownShortOption(string options, IReadOnlyDictionary<char, OptionValueMode> shortOptions)
        {
            foreach (var option in options)
                if (!shortOptions.ContainsKey(option))
                    throw new ParseException($"unknown short option `-{option}`");

            throw new InvalidOperationException("Unreachable short option tokenization state.");
        }

        private static bool IsDigits(string value, int start)
        {
            for (var i = start; i < value.Length; i++)
            {
                var c = value[i];
                if (c < '0' || c > '9')
                    return false;
            }

            return value.Length > start;
        }
    }
}
