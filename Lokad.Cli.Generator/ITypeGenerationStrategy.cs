using System.Collections.Generic;
using Lokad.Cli.Generator.Emit;

namespace Lokad.Cli.Generator
{
    public interface ITypeGenerationStrategy
    {
        CliTypeInfo Type { get; }

        IReadOnlyList<GeneratorException> Diagnostics { get; }

        bool WillEmitSource { get; }

        string? EmitSource();

        bool WillEmitImplementation { get; }

        string? EmitImplementation();
    }
}
