using System;

namespace Lokad.Cli
{
    /// <summary>
    ///     Declares one concrete argument shape that can be selected for a choice interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
    public sealed class ChoiceAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a choice annotation for the specified target type.
        /// </summary>
        /// <param name="type"> Type that may be selected for the annotated choice interface. </param>
        public ChoiceAttribute(Type type)
        {
            Type = type;
        }

        /// <summary>
        ///     Type that may be selected for the annotated choice interface.
        /// </summary>
        public Type Type { get; }
    }
}
