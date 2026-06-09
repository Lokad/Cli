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
        ///     Initializes an argument annotation without a description.
        /// </summary>
        public ArgumentAttribute()
        {
        }

        /// <summary>
        ///     Initializes an argument annotation with user-facing help text.
        /// </summary>
        /// <param name="description"> Description shown in generated help output. </param>
        public ArgumentAttribute(string description)
        {
            Description = description;
        }

        /// <summary>
        ///     User-facing description of the argument.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        ///     Long option name, without the leading <c>--</c>.
        /// </summary>
        public string? Long { get; set; }

        /// <summary>
        ///     Short option name, without the leading <c>-</c>.
        /// </summary>
        public char Short { get; set; }

        /// <summary>
        ///     Indicates whether numeric short-argument forms should be recognized for this argument.
        /// </summary>
        public bool ShortNumeric { get; set; }
    }
}
