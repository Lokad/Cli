#nullable enable
namespace Test;
partial interface IParentChoice : global::Lokad.Cli.ICliParsed<global::Test.IParentChoice>
{
    // A separate holder allows parsing from the argument type's static initializers.
    private static class __CliOptionNames
    {
        public static readonly char[] Short = new char[] { 'g', 'G' };
        public static readonly string[] Long = global::System.Array.Empty<string>();
    }

    static global::Test.IParentChoice global::Lokad.Cli.ICliParsed<global::Test.IParentChoice>.Parse(global::System.Collections.Generic.IReadOnlyList<string> arguments)
    {
        return ParseTokenizedCore(TokenizeCore(arguments));
    }

    static global::Lokad.Cli.TokenizedArguments global::Lokad.Cli.ICliParsed<global::Test.IParentChoice>.Tokenize(global::System.Collections.Generic.IReadOnlyList<string> arguments)
    {
        return TokenizeCore(arguments);
    }

    static bool global::Lokad.Cli.ICliParsed<global::Test.IParentChoice>.IsHelpRequested(global::Lokad.Cli.TokenizedArguments arguments)
    {
        if (arguments == null) throw new global::System.ArgumentNullException(nameof(arguments));

        return false;
    }

    static global::Test.IParentChoice global::Lokad.Cli.ICliParsed<global::Test.IParentChoice>.ParseTokenized(global::Lokad.Cli.TokenizedArguments arguments)
    {
        if (arguments == null) throw new global::System.ArgumentNullException(nameof(arguments));

        return ParseTokenizedCore(arguments);
    }

    static string global::Lokad.Cli.ICliParsed<global::Test.IParentChoice>.Document(string commandName)
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

    private static global::Test.IParentChoice ParseTokenizedCore(global::Lokad.Cli.TokenizedArguments tokenized)
    {
        // Discrimination
        var choice = -1;
        string? choiceDiscriminator = null;
        if (tokenized.HasPositionalPrefix(@"nested"))
        {
            if (choice >= 0)
                throw new global::Lokad.Cli.ParseException($"incompatible options `{choiceDiscriminator}` and `nested`");
            choice = 0;
            choiceDiscriminator = @"nested";
        }

        if (choice < 0)
            throw new global::Lokad.Cli.ParseException(@"expected `nested`");

        switch (choice)
        {
            case 0:
                return ParseChoice<global::Test.NestedDiscriminatorAlias>(tokenized);
        }

        // Binding
        throw new global::System.InvalidOperationException();
    }

    private static string DocumentCore(string commandName)
    {
        if (commandName == null) throw new global::System.ArgumentNullException(nameof(commandName));

        var documentedCommandName = commandName + @" -g";
        var lines = new global::System.Collections.Generic.List<string>();
        lines.Add(FormatDocumentationLine(documentedCommandName, @"Parent discriminator."));
        AddDocumentation(lines, DocumentChoice<global::Test.NestedDiscriminatorAlias>(documentedCommandName));
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
