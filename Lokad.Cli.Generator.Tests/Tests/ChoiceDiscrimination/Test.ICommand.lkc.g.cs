#nullable enable
#pragma warning disable 0162, 0219
namespace Test;
partial interface ICommand : global::Lokad.Cli.ICliParsed<global::Test.ICommand>
{
    static global::Test.ICommand global::Lokad.Cli.ICliParsed<global::Test.ICommand>.Parse(string[] arguments)
    {
        // Tokenization
        var tokenized = global::Lokad.Cli.ArgumentTokenizer.Tokenize(
            arguments,
            new char[] { 'r', 'i' },
            new string[] { @"add", @"name" },
            false);

        return ParseTokenizedCore(tokenized);
    }

    static global::Test.ICommand global::Lokad.Cli.ICliParsed<global::Test.ICommand>.ParseTokenized(global::Lokad.Cli.TokenizedArguments arguments)
    {
        if (arguments == null) throw new global::System.ArgumentNullException(nameof(arguments));

        return ParseTokenizedCore(arguments);
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

    private static TChoice ParseChoice<TChoice>(global::Lokad.Cli.TokenizedArguments arguments)
        where TChoice : global::Lokad.Cli.ICliParsed<TChoice>
    {
        return TChoice.ParseTokenized(arguments);
    }
}
