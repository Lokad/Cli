using System;

namespace Lokad.Cli
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
    public sealed class ChoiceAttribute : Attribute
    {
        public ChoiceAttribute(Type type)
        {
            Type = type;
        }

        public Type Type { get; }
    }
}
