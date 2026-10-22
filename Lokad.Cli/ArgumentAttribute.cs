using System;

namespace Lokad.Cli
{
    /// <summary>
    ///     Configures how a property, field, or constructor parameter is bound from
    ///     command-line arguments.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public sealed class ArgumentAttribute : Attribute
    {
        /// <summary>
        ///     Initializes an argument annotation with user-facing documentation.
        /// </summary>
        /// <param name="documentation"> Documentation shown in generated command documentation. </param>
        public ArgumentAttribute(string documentation)
        {
            Documentation = documentation ?? throw new ArgumentNullException(nameof(documentation));
        }

        /// <summary>
        ///     User-facing documentation of the argument.
        /// </summary>
        public string Documentation { get; }

        /// <summary>
        ///     Positional argument name shown in generated command documentation, without angle brackets.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        ///     Long option name, without the leading <c>--</c>. Mutually exclusive with <see cref="LongNames"/>.
        /// </summary>
        public string? Long { get; set; }

        /// <summary>
        ///     Short option name, without the leading <c>-</c>. Mutually exclusive with <see cref="ShortNames"/>.
        /// </summary>
        public char Short { get; set; }

        /// <summary>
        ///     Complete list of long option names, without leading <c>--</c>.
        ///     Mutually exclusive with <see cref="Long"/>. Aliased arguments use the first name in binding errors.
        /// </summary>
        public string[] LongNames { get; set; } = Array.Empty<string>();

        /// <summary>
        ///     Complete list of short option names, without leading <c>-</c>.
        ///     Mutually exclusive with <see cref="Short"/>. Aliased arguments use the first name in binding errors
        ///     when no long name exists.
        /// </summary>
        public char[] ShortNames { get; set; } = Array.Empty<char>();

        /// <summary>
        ///     Indicates whether numeric short-argument forms should be recognized for this argument.
        /// </summary>
        public bool ShortNumeric { get; set; }

        /// <summary>
        ///     Allows repeated occurrences of a named <see cref="bool"/> or nullable boolean flag.
        ///     Repetition has the same effect as a single occurrence. Defaults to <c>false</c>.
        /// </summary>
        public bool AllowRepeated { get; set; }
    }
}
