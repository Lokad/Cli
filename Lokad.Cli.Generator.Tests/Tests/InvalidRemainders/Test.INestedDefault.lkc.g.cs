#nullable enable
namespace Test;
partial interface INestedDefault : global::Lokad.Cli.ICliParsed<global::Test.INestedDefault>
{
    // A separate holder allows parsing from the argument type's static initializers.
    private static class OptionNames__Cli
    {
        public static readonly global::System.Collections.Generic.IReadOnlyDictionary<char, global::Lokad.Cli.OptionValueMode> Short = new global::System.Collections.Generic.Dictionary<char, global::Lokad.Cli.OptionValueMode> {  };
        public static readonly global::System.Collections.Generic.IReadOnlyDictionary<string, global::Lokad.Cli.OptionValueMode> Long = new global::System.Collections.Generic.Dictionary<string, global::Lokad.Cli.OptionValueMode> {  };
        public static readonly global::Lokad.Cli.CommandBoundary Boundary1 = new global::Lokad.Cli.CommandBoundary(@"run", null, '\0', new global::Lokad.Cli.CommandBoundary[] {  }, null, global::System.Array.Empty<char>(), global::System.Array.Empty<string>(), false);
        public static readonly global::Lokad.Cli.CommandBoundary Boundary = new global::Lokad.Cli.CommandBoundary(null, null, '\0', new global::Lokad.Cli.CommandBoundary[] { Boundary1 }, null, global::System.Array.Empty<char>(), global::System.Array.Empty<string>(), false);
    }

    static global::Test.INestedDefault global::Lokad.Cli.ICliParsed<global::Test.INestedDefault>.Parse(global::System.Collections.Generic.IReadOnlyList<string> arguments)
    {
        return ParseTokenizedCore__Cli(TokenizeCore__Cli(arguments));
    }

    static global::Lokad.Cli.TokenizedArguments global::Lokad.Cli.ICliParsed<global::Test.INestedDefault>.Tokenize(global::System.Collections.Generic.IReadOnlyList<string> arguments)
    {
        return TokenizeCore__Cli(arguments);
    }

    static bool global::Lokad.Cli.ICliParsed<global::Test.INestedDefault>.IsHelpRequested(global::Lokad.Cli.TokenizedArguments arguments)
    {
        if (arguments == null) throw new global::System.ArgumentNullException(nameof(arguments));

        return false;
    }

    static global::Test.INestedDefault global::Lokad.Cli.ICliParsed<global::Test.INestedDefault>.ParseTokenized(global::Lokad.Cli.TokenizedArguments arguments)
    {
        if (arguments == null) throw new global::System.ArgumentNullException(nameof(arguments));

        return ParseTokenizedCore__Cli(arguments);
    }

    static string global::Lokad.Cli.ICliParsed<global::Test.INestedDefault>.Document(string commandName)
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
            true,
            true, OptionNames__Cli.Boundary);
    }

    private static global::Test.INestedDefault ParseTokenizedCore__Cli(global::Lokad.Cli.TokenizedArguments tokenized)
    {
        // Discrimination
        var choice = -1;
        string? choiceDiscriminator = null;
        if (tokenized.HasPositionalPrefix(@"run"))
        {
            if (choice >= 0)
                throw new global::Lokad.Cli.ParseException(@"incompatible options `" + choiceDiscriminator + @"` and `run`");
            choice = 0;
            choiceDiscriminator = @"run";
        }

        if (choice < 0)
            throw new global::Lokad.Cli.ParseException(@"expected `run`");

        switch (choice)
        {
            case 0:
                return ParseChoice__Cli<global::Test.NestedDefaultLeaf>(tokenized);
        }

        // Binding
        throw new global::System.InvalidOperationException();
    }

    private static string DocumentCore__Cli(string commandName)
    {
        if (commandName == null) throw new global::System.ArgumentNullException(nameof(commandName));

        var documentedCommandName = commandName;
        var lines = new global::System.Collections.Generic.List<string>();
        string documentation;
        documentation = DocumentChoice__Cli<global::Test.NestedDefaultLeaf>(documentedCommandName);
        if (documentation.Length > 0)
            lines.Add(documentation);
        return string.Join(global::System.Environment.NewLine, lines);
    }

    private static string DocumentChoice__Cli<TChoice>(string commandName)
        where TChoice : global::Lokad.Cli.ICliParsed<TChoice>
    {
        return TChoice.Document(commandName);
    }

    private static TChoice ParseChoice__Cli<TChoice>(global::Lokad.Cli.TokenizedArguments arguments)
        where TChoice : global::Lokad.Cli.ICliParsed<TChoice>
    {
        return TChoice.ParseTokenized(arguments);
    }
}
