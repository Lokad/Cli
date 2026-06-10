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
        public static TokenizedArguments Tokenize(
            IReadOnlyList<string> arguments,
            char[] shortOptions,
            string[] longOptions,
            bool hasShortNumericArguments)
        {
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));
            if (shortOptions == null) throw new ArgumentNullException(nameof(shortOptions));
            if (longOptions == null) throw new ArgumentNullException(nameof(longOptions));

            var shortSet = new HashSet<char>(shortOptions);
            var longSet = new HashSet<string>(longOptions, StringComparer.Ordinal);
            var tokens = new List<ITokenizedArguments>(arguments.Count);
            var valuesOnly = false;

            foreach (var argument in arguments)
            {
                if (valuesOnly)
                {
                    tokens.Add(new ValueArg(argument));
                    continue;
                }

                if (argument == "--")
                {
                    valuesOnly = true;
                    continue;
                }

                if (TryTokenizeLong(argument, longSet, tokens))
                    continue;

                if (TryTokenizeSingleDashLong(argument, longSet, tokens))
                    continue;

                if (TryTokenizeShort(argument, shortSet, hasShortNumericArguments, tokens))
                    continue;

                tokens.Add(new ValueArg(argument));
            }

            return new TokenizedArguments(tokens);
        }

        /// <summary>
        ///     Attempts to classify an argument as a known long option.
        /// </summary>
        /// <param name="argument"> Raw argument string. </param>
        /// <param name="longOptions"> Recognized long option names. </param>
        /// <param name="tokens"> Destination token list. </param>
        /// <returns>
        ///     <c>true</c> when <paramref name="argument"/> was a long option; otherwise <c>false</c>.
        /// </returns>
        private static bool TryTokenizeLong(
            string argument,
            HashSet<string> longOptions,
            List<ITokenizedArguments> tokens)
        {
            if (!argument.StartsWith("--", StringComparison.Ordinal) || argument.Length <= 2)
                return false;

            var separator = argument.IndexOf('=');
            var name = separator < 0 ? argument.Substring(2) : argument.Substring(2, separator - 2);

            if (!longOptions.Contains(name))
                throw new ParseException($"unknown option `--{name}`");

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

        private static bool TryTokenizeSingleDashLong(
            string argument,
            HashSet<string> longOptions,
            List<ITokenizedArguments> tokens)
        {
            if (!argument.StartsWith("-", StringComparison.Ordinal) ||
                argument.StartsWith("--", StringComparison.Ordinal) ||
                argument.Length <= 2)
            {
                return false;
            }

            var separator = argument.IndexOf('=');
            var name = separator < 0 ? argument.Substring(1) : argument.Substring(1, separator - 1);

            if (!longOptions.Contains(name))
                return false;

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
            HashSet<char> shortOptions,
            bool hasShortNumericArguments,
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
            var separator = set.IndexOf('=');
            if (separator >= 0)
            {
                if (separator == 1)
                {
                    var option = set[0];
                    if (!shortOptions.Contains(option))
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

            if (set.Length > 1 && shortOptions.Contains(set[0]) && IsDigits(set, 1))
            {
                tokens.Add(new ShortArgWithValue(set[0], set.Substring(1)));
                return true;
            }

            ThrowUnknownShortOption(set, shortOptions);
            return true;
        }

        private static bool AllShortOptionsKnown(string options, HashSet<char> shortOptions)
        {
            foreach (var option in options)
                if (!shortOptions.Contains(option))
                    return false;

            return true;
        }

        private static void ThrowUnknownShortOption(string options, HashSet<char> shortOptions)
        {
            foreach (var option in options)
                if (!shortOptions.Contains(option))
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
