namespace Lokad.Cli
{
    /// <summary>Describes whether a tokenized option takes a following value.</summary>
    public enum OptionValueMode
    {
        /// <summary>A flag, or an option whose arity is unspecified by the legacy names-only API.</summary>
        None,

        /// <summary>Requires a value following the normal option and terminator rules.</summary>
        Required,

        /// <summary>Requires the next raw argument verbatim, including a leading dash or a literal <c>--</c>.</summary>
        Verbatim
    }
}
