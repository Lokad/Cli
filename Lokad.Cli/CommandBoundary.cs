using System;
using System.Collections.Generic;

namespace Lokad.Cli
{
    /// <summary>Generated command selectors and literal-suffix boundaries for tokenization.</summary>
    public sealed class CommandBoundary
    {
        /// <summary>An argument shape with no selectors or explicit literal boundary.</summary>
        public static readonly CommandBoundary Empty = new CommandBoundary(null, null, '\0',
            Array.Empty<CommandBoundary>(), null, Array.Empty<char>(), Array.Empty<string>());

        /// <summary>Creates a command's lexical boundary schema. Input collections must remain unchanged.</summary>
        /// <param name="prefix">The command's positional discriminator, or null.</param>
        /// <param name="longOption">The command's long discriminator, or null.</param>
        /// <param name="shortOption">The command's short discriminator, or '\0'.</param>
        /// <param name="choices">Direct child choices, in declaration order.</param>
        /// <param name="remainderAfterOperands">Number of scalar operands before a remainder; zero waits for an option boundary, <c>--</c> or the first-operand policy; null declares no remainder.</param>
        /// <param name="stopAfterShortOptions">Short options whose required value ends option parsing.</param>
        /// <param name="stopAfterLongOptions">Long options whose required value ends option parsing.</param>
        public CommandBoundary(string? prefix, string? longOption, char shortOption,
            IReadOnlyList<CommandBoundary> choices, int? remainderAfterOperands,
            IReadOnlyList<char> stopAfterShortOptions, IReadOnlyList<string> stopAfterLongOptions)
        {
            Prefix = prefix;
            LongOption = longOption;
            ShortOption = shortOption;
            Choices = choices;
            RemainderAfterOperands = remainderAfterOperands;
            StopAfterShortOptions = stopAfterShortOptions;
            StopAfterLongOptions = stopAfterLongOptions;
            HasBoundaries = remainderAfterOperands.HasValue || stopAfterShortOptions.Count > 0 || stopAfterLongOptions.Count > 0;
            HasShortBoundaries = stopAfterShortOptions.Count > 0;
            foreach (var choice in choices)
            {
                HasBoundaries |= choice.HasBoundaries;
                HasShortBoundaries |= choice.HasShortBoundaries;
            }
        }

        /// <summary>The command's positional discriminator, or null.</summary>
        public string? Prefix { get; }
        /// <summary>The command's long discriminator, or null.</summary>
        public string? LongOption { get; }
        /// <summary>The command's short discriminator, or '\0'.</summary>
        public char ShortOption { get; }
        /// <summary>The command's direct choices.</summary>
        public IReadOnlyList<CommandBoundary> Choices { get; }
        /// <summary>The number of scalar operands before a remainder; null means none is declared.</summary>
        public int? RemainderAfterOperands { get; }
        /// <summary>Short options whose required value ends option parsing.</summary>
        public IReadOnlyList<char> StopAfterShortOptions { get; }
        /// <summary>Long options whose required value ends option parsing.</summary>
        public IReadOnlyList<string> StopAfterLongOptions { get; }
        internal bool HasBoundaries { get; }
        internal bool HasShortBoundaries { get; }
        internal bool IsDefault => Prefix == null && LongOption == null && ShortOption == '\0';
    }
}
