using System;

namespace Lokad.Cli
{
    /// <summary>
    ///     Marks a type as a command-line argument shape that can be parsed by the generator.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public sealed class ArgumentsAttribute : Attribute
    {
        /// <summary>
        ///     Initializes an argument shape annotation with user-facing documentation.
        /// </summary>
        /// <param name="documentation"> Documentation shown in generated command documentation. </param>
        public ArgumentsAttribute(string documentation)
        {
            Documentation = documentation ?? throw new ArgumentNullException(nameof(documentation));
        }

        /// <summary>
        ///     User-facing documentation of the argument shape.
        /// </summary>
        public string Documentation { get; }

        /// <summary>
        ///     Positional discriminator prefix required for this argument shape.
        /// </summary>
        public string? Prefix { get; set; }

        /// <summary>
        ///     Long-option discriminator, without the leading <c>--</c>.
        /// </summary>
        public string? Long { get; set; }

        /// <summary>
        ///     Short-option discriminator, without the leading <c>-</c>.
        /// </summary>
        public char Short { get; set; }
    }
}
