#nullable enable
namespace Test;
partial interface ICountGroupPolicy : global::Lokad.Cli.ICliParsed<global::Test.ICountGroupPolicy>
{
    // A separate holder allows parsing from the argument type's static initializers.
    private static class OptionNames__Cli
    {
        public static readonly global::System.Collections.Generic.IReadOnlyDictionary<char, global::Lokad.Cli.OptionValueMode> Short = new global::System.Collections.Generic.Dictionary<char, global::Lokad.Cli.OptionValueMode> {  };
        public static readonly global::System.Collections.Generic.IReadOnlyDictionary<string, global::Lokad.Cli.OptionValueMode> Long = new global::System.Collections.Generic.Dictionary<string, global::Lokad.Cli.OptionValueMode> { [@"count"] = global::Lokad.Cli.OptionValueMode.None };
    }

    static global::Test.ICountGroupPolicy global::Lokad.Cli.ICliParsed<global::Test.ICountGroupPolicy>.Parse(global::System.Collections.Generic.IReadOnlyList<string> arguments)
    {
        return ParseTokenizedCore__Cli(TokenizeCore__Cli(arguments));
    }

    static global::Lokad.Cli.TokenizedArguments global::Lokad.Cli.ICliParsed<global::Test.ICountGroupPolicy>.Tokenize(global::System.Collections.Generic.IReadOnlyList<string> arguments)
    {
        return TokenizeCore__Cli(arguments);
    }

    static bool global::Lokad.Cli.ICliParsed<global::Test.ICountGroupPolicy>.IsHelpRequested(global::Lokad.Cli.TokenizedArguments arguments)
    {
        if (arguments == null) throw new global::System.ArgumentNullException(nameof(arguments));

        return false;
    }

    static global::Test.ICountGroupPolicy global::Lokad.Cli.ICliParsed<global::Test.ICountGroupPolicy>.ParseTokenized(global::Lokad.Cli.TokenizedArguments arguments)
    {
        if (arguments == null) throw new global::System.ArgumentNullException(nameof(arguments));

        return ParseTokenizedCore__Cli(arguments);
    }

    static string global::Lokad.Cli.ICliParsed<global::Test.ICountGroupPolicy>.Document(string commandName)
    {
        return DocumentCore__Cli(commandName);
    }

    private static global::Lokad.Cli.TokenizedArguments TokenizeCore__Cli(global::System.Collections.Generic.IReadOnlyList<string> arguments)
    {
        return global::Lokad.Cli.ArgumentTokenizer.Tokenize(
            arguments,
            OptionNames__Cli.Short,
            OptionNames__Cli.Long,
            false,
            global::Lokad.Cli.ShortOptionSyntax.GroupedValues,
            true);
    }

    private static global::Test.ICountGroupPolicy ParseTokenizedCore__Cli(global::Lokad.Cli.TokenizedArguments tokenized)
    {
        // Discrimination
        var choice = -1;

        if (choice < 0)
            choice = 0;

        switch (choice)
        {
            case 0:
                return ParseChoice__Cli<global::Test.CountGroup>(tokenized);
        }

        // Binding
        throw new global::System.InvalidOperationException();
    }

    private static string DocumentCore__Cli(string commandName)
    {
        if (commandName == null) throw new global::System.ArgumentNullException(nameof(commandName));

        var documentedCommandName = commandName;
        var lines = new global::System.Collections.Generic.List<string>();
        lines.Add(FormatDocumentationLine__Cli(documentedCommandName, @"Count policy."));
        string documentation;
        documentation = DocumentChoice__Cli<global::Test.CountGroup>(documentedCommandName);
        if (documentation.Length > 0)
            lines.Add(documentation);
        return string.Join(global::System.Environment.NewLine, lines);
    }

    private static string DocumentChoice__Cli<TChoice>(string commandName)
        where TChoice : global::Lokad.Cli.ICliParsed<TChoice>
    {
        return TChoice.Document(commandName);
    }

    private static string FormatDocumentationLine__Cli(string commandName, string documentation)
    {
        if (documentation.Length == 0)
            return commandName;

        return commandName + ": " + documentation;
    }

    private static TChoice ParseChoice__Cli<TChoice>(global::Lokad.Cli.TokenizedArguments arguments)
        where TChoice : global::Lokad.Cli.ICliParsed<TChoice>
    {
        return TChoice.ParseTokenized(arguments);
    }
}
