using System.Collections.Generic;
using Lokad.Cli.Generator.Emit;

namespace Lokad.Cli.Generator
{
    public interface ITypeGenerationStrategy
    {
        CliTypeInfo Type { get; }

        IReadOnlyList<GeneratorException> Diagnostics { get; }

        /// <summary>Returns generated source, or null when only diagnostics should be emitted.</summary>
        string? EmitSource();
    }
}
