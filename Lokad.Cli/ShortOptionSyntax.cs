namespace Lokad.Cli
{
    /// <summary>Controls how a short option group supplies values.</summary>
    public enum ShortOptionSyntax
    {
        /// <summary>Preserves grouped operands such as <c>-ee first second</c> and the legacy inline-value heuristic.</summary>
        GroupedValues,

        /// <summary>The first value-taking option consumes the remainder of its argument, or a following operand.</summary>
        AttachedValues
    }
}
