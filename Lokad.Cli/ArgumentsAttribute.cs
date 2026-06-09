using System;

namespace Lokad.Cli
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public sealed class ArgumentsAttribute : Attribute
    {
        public string? Prefix { get; set; }

        public string? Long { get; set; }

        public char Short { get; set; }
    }
}
