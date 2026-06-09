using System;

namespace Lokad.Cli
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public sealed class ArgumentAttribute : Attribute
    {
        public ArgumentAttribute()
        {
        }

        public ArgumentAttribute(string description)
        {
            Description = description;
        }

        public string? Description { get; }

        public string? Long { get; set; }

        public char Short { get; set; }

        public bool ShortNumeric { get; set; }
    }
}
