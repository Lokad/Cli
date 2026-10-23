using System.Collections.Generic;

namespace Lokad.Cli.Generator.Emit
{
    public sealed class CliOptionNames
    {
        public CliOptionNames(
            IReadOnlyList<string> longOptions,
            IReadOnlyList<char> shortOptions)
        {
            LongOptions = longOptions;
            ShortOptions = shortOptions;
            HasAliases = longOptions.Count > 1 || shortOptions.Count > 1;
            HasNamedOption = shortOptions.Count > 0;

            var labels = new List<string>();
            foreach (var name in longOptions)
            {
                HasNamedOption |= name.Length > 0;
                labels.Add("--" + name);
            }
            foreach (var name in shortOptions)
                labels.Add("-" + name);
            Labels = labels;

            PrimaryLabel = labels.Count > 0 ? labels[0] : "argument";
        }

        public IReadOnlyList<string> LongOptions { get; }

        public IReadOnlyList<char> ShortOptions { get; }

        public IReadOnlyList<string> Labels { get; }

        public string PrimaryLabel { get; }

        public bool HasAliases { get; }

        public bool HasNamedOption { get; }
    }
}
