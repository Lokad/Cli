#nullable enable
namespace Test;
partial interface IParentChoice : global::Lokad.Cli.ICliParsed<global::Test.IParentChoice>
{
    // A separate holder allows parsing from the argument type's static initializers.
    private static class OptionNames__Cli
    {
        public static readonly global::System.Collections.Generic.IReadOnlyDictionary<char, global::Lokad.Cli.OptionValueMode> Short = new global::System.Collections.Generic.Dictionary<char, global::Lokad.Cli.OptionValueMode> { ['g'] = global::Lokad.Cli.OptionValueMode.None, ['G'] = global::Lokad.Cli.OptionValueMode.None };
        public static readonly global::System.Collections.Generic.IReadOnlyDictionary<string, global::Lokad.Cli.OptionValueMode> Long = new global::System.Collections.Generic.Dictionary<string, global::Lokad.Cli.OptionValueMode> {  };
    }

    static global::Test.IParentChoice global::Lokad.Cli.ICliParsed<global::Test.IParentChoice>.Parse(global::System.Collections.Generic.IReadOnlyList<string> arguments)
    {
        return ParseTokenizedCore__Cli(TokenizeCore__Cli(arguments));
    }

    static global::Lokad.Cli.TokenizedArguments global::Lokad.Cli.ICliParsed<global::Test.IParentChoice>.Tokenize(global::System.Collections.Generic.IReadOnlyList<string> arguments)
    {
        return TokenizeCore__Cli(arguments);
    }

    static bool global::Lokad.Cli.ICliParsed<global::Test.IParentChoice>.IsHelpRequested(global::Lokad.Cli.TokenizedArguments arguments)
    {
        if (arguments == null) throw new global::System.ArgumentNullException(nameof(arguments));

        return false;
    }

    static global::Test.IParentChoice global::Lokad.Cli.ICliParsed<global::Test.IParentChoice>.ParseTokenized(global::Lokad.Cli.TokenizedArguments arguments)
    {
        if (arguments == null) throw new global::System.ArgumentNullException(nameof(arguments));

        return ParseTokenizedCore__Cli(arguments);
    }

    static string global::Lokad.Cli.ICliParsed<global::Test.IParentChoice>.Document(string commandName)
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

    private static global::Test.IParentChoice ParseTokenizedCore__Cli(global::Lokad.Cli.TokenizedArguments tokenized)
    {
        tokenized.SelectDiscriminator(null, 'g');

        // Discrimination
        var choice = -1;
        string? choiceDiscriminator = null;
        if (tokenized.HasPositionalPrefix(@"nested"))
        {
            if (choice >= 0)
                throw new global::Lokad.Cli.ParseException(@"incompatible options `" + choiceDiscriminator + @"` and `nested`");
            choice = 0;
            choiceDiscriminator = @"nested";
        }

        if (choice < 0)
            throw new global::Lokad.Cli.ParseException(@"expected `nested`");

        switch (choice)
        {
            case 0:
                return ParseChoice__Cli<global::Test.NestedDiscriminatorAlias>(tokenized);
        }

        // Binding
        throw new global::System.InvalidOperationException();
    }

    private static string DocumentCore__Cli(string commandName)
    {
        if (commandName == null) throw new global::System.ArgumentNullException(nameof(commandName));

        var documentedCommandName = commandName + @" -g";
        var lines = new global::System.Collections.Generic.List<string>();
        lines.Add(FormatDocumentationLine__Cli(documentedCommandName, @"Parent discriminator."));
        AddDocumentation__Cli(lines, DocumentChoice__Cli<global::Test.NestedDiscriminatorAlias>(documentedCommandName));
        return string.Join(global::System.Environment.NewLine, lines);
    }

    private static string DocumentChoice__Cli<TChoice>(string commandName)
        where TChoice : global::Lokad.Cli.ICliParsed<TChoice>
    {
        return TChoice.Document(commandName);
    }

    private static void AddDocumentation__Cli(global::System.Collections.Generic.List<string> lines, string documentation)
    {
        if (documentation.Length == 0)
            return;

        var split = documentation.Split(new[] { global::System.Environment.NewLine }, global::System.StringSplitOptions.None);
        foreach (var line in split)
            lines.Add(line);
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
