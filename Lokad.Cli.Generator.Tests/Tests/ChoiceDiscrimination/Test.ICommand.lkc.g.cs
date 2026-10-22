#nullable enable
namespace Test;
partial interface ICommand : global::Lokad.Cli.ICliParsed<global::Test.ICommand>
{
    // A separate holder allows parsing from the argument type's static initializers.
    private static class __CliOptionNames
    {
        public static readonly char[] Short = new char[] { 'r', 'i' };
        public static readonly string[] Long = new string[] { @"add", @"name" };
    }

    static global::Test.ICommand global::Lokad.Cli.ICliParsed<global::Test.ICommand>.Parse(global::System.Collections.Generic.IReadOnlyList<string> arguments)
    {
        return ParseTokenizedCore(TokenizeCore(arguments));
    }

    static global::Lokad.Cli.TokenizedArguments global::Lokad.Cli.ICliParsed<global::Test.ICommand>.Tokenize(global::System.Collections.Generic.IReadOnlyList<string> arguments)
    {
        return TokenizeCore(arguments);
    }

    static global::Test.ICommand global::Lokad.Cli.ICliParsed<global::Test.ICommand>.ParseTokenized(global::Lokad.Cli.TokenizedArguments arguments)
    {
        if (arguments == null) throw new global::System.ArgumentNullException(nameof(arguments));

        return ParseTokenizedCore(arguments);
    }

    static string global::Lokad.Cli.ICliParsed<global::Test.ICommand>.Document(string commandName)
    {
        return DocumentCore(commandName);
    }

    private static global::Lokad.Cli.TokenizedArguments TokenizeCore(global::System.Collections.Generic.IReadOnlyList<string> arguments)
    {
        return global::Lokad.Cli.ArgumentTokenizer.Tokenize(
            arguments,
            __CliOptionNames.Short,
            __CliOptionNames.Long,
            false);
    }

    private static global::Test.ICommand ParseTokenizedCore(global::Lokad.Cli.TokenizedArguments tokenized)
    {
        // Discrimination
        var choice = -1;
        string? choiceDiscriminator = null;
        if (tokenized.HasLongOption(@"add"))
        {
            if (choice >= 0)
                throw new global::Lokad.Cli.ParseException($"incompatible options `{choiceDiscriminator}` and `--add`");
            choice = 0;
            choiceDiscriminator = @"--add";
        }
        if (tokenized.HasShortOption('r'))
        {
            if (choice >= 0)
                throw new global::Lokad.Cli.ParseException($"incompatible options `{choiceDiscriminator}` and `-r`");
            choice = 1;
            choiceDiscriminator = @"-r";
        }
        if (tokenized.HasPositionalPrefix(@"list"))
        {
            if (choice >= 0)
                throw new global::Lokad.Cli.ParseException($"incompatible options `{choiceDiscriminator}` and `list`");
            choice = 2;
            choiceDiscriminator = @"list";
        }

        if (choice < 0)
            throw new global::Lokad.Cli.ParseException(@"expected `--add`, `-r` or `list`");

        switch (choice)
        {
            case 0:
                return ParseChoice<global::Test.AddCommand>(tokenized);
            case 1:
                return ParseChoice<global::Test.RemoveCommand>(tokenized);
            case 2:
                return ParseChoice<global::Test.ListCommand>(tokenized);
        }

        // Binding
        throw new global::System.InvalidOperationException();
    }

    private static string DocumentCore(string commandName)
    {
        if (commandName == null) throw new global::System.ArgumentNullException(nameof(commandName));

        var documentedCommandName = commandName;
        var lines = new global::System.Collections.Generic.List<string>();
        AddDocumentation(lines, DocumentChoice<global::Test.AddCommand>(documentedCommandName));
        AddDocumentation(lines, DocumentChoice<global::Test.RemoveCommand>(documentedCommandName));
        AddDocumentation(lines, DocumentChoice<global::Test.ListCommand>(documentedCommandName));
        return string.Join(global::System.Environment.NewLine, lines);
    }

    private static string DocumentChoice<TChoice>(string commandName)
        where TChoice : global::Lokad.Cli.ICliParsed<TChoice>
    {
        return TChoice.Document(commandName);
    }

    private static void AddDocumentation(global::System.Collections.Generic.List<string> lines, string documentation)
    {
        if (documentation.Length == 0)
            return;

        var split = documentation.Split(new[] { global::System.Environment.NewLine }, global::System.StringSplitOptions.None);
        foreach (var line in split)
            lines.Add(line);
    }

    private static string FormatDocumentationLine(string commandName, string documentation)
    {
        if (documentation.Length == 0)
            return commandName;

        return commandName + ": " + documentation;
    }

    private static TChoice ParseChoice<TChoice>(global::Lokad.Cli.TokenizedArguments arguments)
        where TChoice : global::Lokad.Cli.ICliParsed<TChoice>
    {
        return TChoice.ParseTokenized(arguments);
    }
}
