using System.Collections.Generic;

namespace Lokad.Cli.Generator.Emit
{
    public sealed class CliCommandBoundaryInfo
    {
        public CliCommandBoundaryInfo(CliDiscriminatorInfo? discriminator,
            IReadOnlyList<CliCommandBoundaryInfo> choices, int? remainderAfterOperands,
            IReadOnlyList<char> stopAfterShortOptions, IReadOnlyList<string> stopAfterLongOptions)
        {
            Discriminator = discriminator;
            Choices = choices;
            RemainderAfterOperands = remainderAfterOperands;
            StopAfterShortOptions = stopAfterShortOptions;
            StopAfterLongOptions = stopAfterLongOptions;
        }

        public CliDiscriminatorInfo? Discriminator { get; }
        public IReadOnlyList<CliCommandBoundaryInfo> Choices { get; }
        public int? RemainderAfterOperands { get; }
        public IReadOnlyList<char> StopAfterShortOptions { get; }
        public IReadOnlyList<string> StopAfterLongOptions { get; }
    }
}
